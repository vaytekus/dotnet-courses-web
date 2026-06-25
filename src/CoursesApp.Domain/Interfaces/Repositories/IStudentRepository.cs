using CoursesApp.Domain.Entities;

namespace CoursesApp.Domain.Interfaces
{
    public interface IStudentRepository
    {
        Task<List<Student>> GetAllStudentsAsync();
        Task<List<Student>> GetStudentsByGroupAsync(Guid groupId);
        Task<Student?> GetStudentByIdAsync(Guid id);
        void UpdateStudent(Student student);
        void DeleteStudent(Student student);
    }
}