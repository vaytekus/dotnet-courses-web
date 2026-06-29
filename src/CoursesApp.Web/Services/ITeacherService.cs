using CoursesApp.Web.DTOs;

namespace CoursesApp.Web.Services
{
    public interface ITeacherService
    {
        Task AddTeacherAsync(TeacherDto teacher);
        Task UpdateTeacherAsync(TeacherEditDto dto);
        Task DeleteTeacherAsync(Guid id);
    }
}