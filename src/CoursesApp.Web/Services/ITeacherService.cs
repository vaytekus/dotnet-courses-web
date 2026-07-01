using CoursesApp.Web.DTOs;
using CoursesApp.Web.Models;

namespace CoursesApp.Web.Services
{
    public interface ITeacherService
    {
        Task<TeachersIndexViewModel> GetPageAsync(string? search, int page, int pageSize);
        Task AddTeacherAsync(TeacherDto teacher);
        Task UpdateTeacherAsync(TeacherEditDto dto);
        Task DeleteTeacherAsync(Guid id);
    }
}