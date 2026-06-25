using CoursesApp.Domain.Entities;

namespace CoursesApp.Domain.Interfaces
{
    public interface ICourseRepository
    {
        Task<List<Course>> GetAllCoursesAsync();
        Task<List<Course>> GetAllCoursesWithDetailsAsync();
        Task<List<Course>> SearchCoursesAsync(string query);
    }
}