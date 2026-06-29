using CoursesApp.Domain.Entities;

namespace CoursesApp.Domain.Interfaces
{
    public interface IStudentRepository
    {
        Task<List<Student>> GetStudentsByGroupAsync(Guid groupId);
        Task<Student?> GetStudentByIdAsync(Guid id);
        Task<(List<Student>, int TotalCount)> GetFilteredStudentAsync(string? searchQuery, Guid? groupId, int page, int pageSize);
        void AddStudent(Student student);
        void UpdateStudent(Student student);
        void DeleteStudent(Student student);
    }
}