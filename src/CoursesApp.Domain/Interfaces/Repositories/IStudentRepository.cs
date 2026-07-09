using CoursesApp.Domain.Entities;

namespace CoursesApp.Domain.Interfaces.Repositories;

public interface IStudentRepository : IRepository<Student>
{
    Task<List<Student>> GetStudentsByGroupAsync(Guid groupId, CancellationToken ct = default);
    Task<(List<Student>, int TotalCount)> GetFilteredStudentAsync(string? searchQuery, Guid? groupId, int page, int pageSize, CancellationToken ct = default);
    Task DeleteAllByGroupAsync(Guid groupId, CancellationToken ct = default);
}
