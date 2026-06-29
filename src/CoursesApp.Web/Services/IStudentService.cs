using CoursesApp.Web.DTOs;

namespace CoursesApp.Web.Services
{
    public interface IStudentService
    {
        Task AddStudentAsync(StudentDto studentDto);
        Task UpdateStudentAsync(StudentEditDto dto);
        Task DeleteStudentAsync(Guid id);
    }
}