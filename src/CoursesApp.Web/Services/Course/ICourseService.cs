using CoursesApp.Domain.Entities;

namespace CoursesApp.Web.Services
{
    public interface ICourseService
    {
        Task<List<Course>> GetAllWithDetailsAsync(CancellationToken ct = default);
    }
}