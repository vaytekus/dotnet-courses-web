namespace CoursesApp.Web.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PaginationOptions>(configuration.GetSection("Pagination"));
        services.Configure<EmailOptions>(configuration.GetSection("Email"));
        services.AddMemoryCache();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddSingleton<ICsvLineParser, NumberedCsvLineParser>();
        services.AddSingleton<ICsvLineParser, SimpleCsvLineParser>();
        services.AddScoped<ICsvService, CsvService>();
        return services;
    }
}
