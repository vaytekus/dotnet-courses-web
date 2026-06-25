using CoursesApp.Web.DTOs;

namespace CoursesApp.Web.Services
{
    public interface ITeacherService
    {
        Task UpdateTeacherAsync(TeacherEditDto dto);
        Task DeleteTeacherAsync(Guid id);
    }
}