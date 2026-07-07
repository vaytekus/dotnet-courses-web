namespace CoursesApp.Web.Services;

public interface ICourseService
{
    Task<List<Course>> GetAllWithDetailsAsync(CancellationToken ct = default);
}
