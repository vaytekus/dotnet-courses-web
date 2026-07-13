using System.Threading.RateLimiting;
using CoursesApp.Infrastructure.Data;
using CoursesApp.Infrastructure.Extensions;
using CoursesApp.Web.Extensions;
using CoursesApp.Web.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Serilog;

const int RateLimitWindowMinutes = 1;
const int RateLimitPermitLimit = 100;
const int RateLimitQueueLimit = 10;
const int CookieExpireDays = 14;
const int RegisterRateLimitWindowMinutes = 10;
const int RegisterRateLimitPermitLimit = 5;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    builder.Services.AddControllersWithViews(options =>
    {
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        options.Filters.Add(new AuthorizeFilter(policy));
    })
    .AddRazorRuntimeCompilation();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddWebServices(builder.Configuration);
    builder.Services.AddSignalR();
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromMinutes(RateLimitWindowMinutes),
                    PermitLimit = RateLimitPermitLimit,
                    QueueLimit = RateLimitQueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                }
            )
        );

        options.AddPolicy(RateLimiterPolicyNames.Register, context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromMinutes(RegisterRateLimitWindowMinutes),
                    PermitLimit = RegisterRateLimitPermitLimit,
                    QueueLimit = 0,
                    AutoReplenishment = true
                }
            )
        );

        options.OnRejected = async (context, ct) =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>(); 
            logger.LogWarning("Rate limit exceeded for {IP} on {Method} {Path}",
                context.HttpContext.Connection.RemoteIpAddress,
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path);

            if (context.HttpContext.Request.Path.StartsWithSegments("/auth/register"))
            {
                context.HttpContext.Response.Redirect("/auth/register?rateLimited=true");
                return;
            }

            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
            await context.HttpContext.Response.WriteAsync("Too many requests. Please try again in a few minutes.", ct);
        };
    });
    builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.AccessDeniedPath = "/auth/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(CookieExpireDays);
        options.SlidingExpiration = true;
    });

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await DbSeeder.SeedAsync(db);
    }

    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();

    app.MapStaticAssets();

    app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Courses}/{action=Index}/{id?}")
        .WithStaticAssets();

    app.MapHub<AppHub>("/hubs/app");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.CloseAndFlush();
}
