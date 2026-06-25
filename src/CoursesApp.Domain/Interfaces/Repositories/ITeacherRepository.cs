using CoursesApp.Domain.Entities;

namespace CoursesApp.Domain.Interfaces
{
    public interface ITeacherRepository
    {
        Task<List<Teacher>> GetAllTeachersAsync();
        Task<Teacher?> GetTeacherByIdAsync(Guid id);
        void UpdateTeacher(Teacher teacher);
        void DeleteTeacher(Teacher teacher);
    }
}