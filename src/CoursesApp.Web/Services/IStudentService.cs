using CoursesApp.Web.DTOs;

namespace CoursesApp.Web.Services
{
    public interface IStudentService
    {
        Task UpdateStudentAsync(StudentEditDto dto);
        Task DeleteStudentAsync(Guid id);
    }
}