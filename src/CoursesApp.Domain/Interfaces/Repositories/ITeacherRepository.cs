using CoursesApp.Domain.Entities;

namespace CoursesApp.Domain.Interfaces.Repositories
{
    public interface ITeacherRepository
    {
        Task<List<Teacher>> GetAllTeachersAsync(CancellationToken ct = default);
        Task<(List<Teacher>, int TotalCount)> GetFilteredTeachersAsync(string? search, int page, int pageSize, CancellationToken ct = default);
        Task<Teacher?> GetTeacherByIdAsync(Guid id, CancellationToken ct = default);
        void AddTeacher(Teacher teacher);
        void UpdateTeacher(Teacher teacher);
        void DeleteTeacher(Teacher teacher);
    }
}