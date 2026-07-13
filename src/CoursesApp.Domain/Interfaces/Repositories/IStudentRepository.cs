using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Enums;

namespace CoursesApp.Domain.Interfaces.Repositories;

public interface IStudentRepository : IRepository<Student>
{
    Task<List<Student>> GetStudentsByGroupAsync(Guid groupId, CancellationToken ct = default);
    Task<(List<Student>, int TotalCount)> GetFilteredStudentAsync(
        string? searchQuery, Guid? groupId, int page, int pageSize, StudentSortKey sortKey, bool sortDesc, CancellationToken ct = default);
    Task DeleteAllByGroupAsync(Guid groupId, CancellationToken ct = default);
}
