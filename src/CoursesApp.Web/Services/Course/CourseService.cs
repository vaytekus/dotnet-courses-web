namespace CoursesApp.Web.Services;

public class CourseService(
    IUnitOfWork uow) : ICourseService
{
    private readonly IUnitOfWork _uow = uow;
    public async Task<List<Course>> GetAllWithDetailsAsync(CancellationToken ct = default)
    {
        return await _uow.Courses.GetAllCoursesWithDetailsAsync(ct);
    }
}
