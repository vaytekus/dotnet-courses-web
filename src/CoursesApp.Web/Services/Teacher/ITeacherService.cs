using CoursesApp.Web.DTOs;
using CoursesApp.Web.Models;

namespace CoursesApp.Web.Services
{
    public interface ITeacherService
    {
        Task<TeachersIndexViewModel> GetPageAsync(string? search, int page, int pageSize, CancellationToken ct = default);
        Task AddTeacherAsync(TeacherDto teacher, CancellationToken ct = default);
        Task UpdateTeacherAsync(TeacherEditDto dto, CancellationToken ct = default);
        Task ValidateExistAsync(Guid id, CancellationToken ct = default);
        Task DeleteTeacherAsync(Guid id, CancellationToken ct = default);
        
    }
}