using CoursesApp.Web.DTOs;
using CoursesApp.Web.Models;

namespace CoursesApp.Web.Services
{
    public interface IStudentService
    {
        Task<StudentsIndexViewModel> GetPageAsync(string? search, Guid? id, int page, int pageSize);
        Task<List<StudentDto>> GetStudentsByGroupAsync(Guid groupId);
        Task AddStudentAsync(StudentDto studentDto);
        Task UpdateStudentAsync(StudentEditDto dto);
        Task DeleteStudentAsync(Guid id);
        Task ClearAllStudentsAsync(Guid id);
        Task<byte[]> ExportGroupCsvAsync(Guid groupId);
        Task<ImportResult> ImportGroupCsvAsync(Stream stream, Guid groupId);
    }
}