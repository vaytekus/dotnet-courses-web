using CoursesApp.Domain.Entities;

namespace CoursesApp.Domain.Interfaces.Repositories;

public interface IStudentRepository
{
    Task<List<Student>> GetStudentsByGroupAsync(Guid groupId, CancellationToken ct = default);
    Task<Student?> GetStudentByIdAsync(Guid id, CancellationToken ct = default);
    Task<(List<Student>, int TotalCount)> GetFilteredStudentAsync(string? searchQuery, Guid? groupId, int page, int pageSize, CancellationToken ct = default);
    void AddStudent(Student student);
    void UpdateStudent(Student student);
    void DeleteStudent(Student student);
    Task DeleteAllByGroupAsync(Guid groupId, CancellationToken ct = default);
}
