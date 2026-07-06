using CoursesApp.Web.DTOs;
using CoursesApp.Web.Models;

namespace CoursesApp.Web.Services
{
    public interface IStudentService
    {
        Task<StudentsIndexViewModel> GetPageAsync(string? search, Guid? id, int page, int pageSize, CancellationToken ct = default);
        Task<List<StudentDto>> GetStudentsByGroupAsync(Guid groupId, CancellationToken ct = default);
        Task AddStudentAsync(StudentDto studentDto, CancellationToken ct = default);
        Task UpdateStudentAsync(StudentEditDto dto, CancellationToken ct = default);
        Task DeleteStudentAsync(Guid id, CancellationToken ct = default);
        Task ClearAllStudentsAsync(Guid id, CancellationToken ct = default);
    }
}