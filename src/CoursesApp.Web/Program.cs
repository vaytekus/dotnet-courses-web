using System.Threading.RateLimiting;
using CoursesApp.Infrastructure.Data;
using CoursesApp.Infrastructure.Extensions;
using CoursesApp.Web.Extensions;
using CoursesApp.Web.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;

const int RateLimitWindowMinutes = 1;
const int RateLimitPermitLimit = 100;
const int RateLimitQueueLimit = 10;
const int CookieExpireDays = 14;

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
        options.AddFixedWindowLimiter(RateLimiterPolicyNames.Fixed, limiter =>
        {
            limiter.Window = TimeSpan.FromMinutes(RateLimitWindowMinutes);
            limiter.PermitLimit = RateLimitPermitLimit;
            limiter.QueueLimit = RateLimitQueueLimit;
            limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
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
        .WithStaticAssets()
        .RequireRateLimiting(RateLimiterPolicyNames.Fixed);

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
