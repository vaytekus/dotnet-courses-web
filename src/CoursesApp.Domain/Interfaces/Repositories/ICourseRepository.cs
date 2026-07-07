using CoursesApp.Domain.Entities;

namespace CoursesApp.Domain.Interfaces.Repositories;

public interface ICourseRepository
{
    Task<List<Course>> GetAllCoursesAsync(CancellationToken ct = default);
    Task<List<Course>> GetAllCoursesWithDetailsAsync(CancellationToken ct = default);
    Task<List<Course>> SearchCoursesAsync(string query, CancellationToken ct = default);
}
