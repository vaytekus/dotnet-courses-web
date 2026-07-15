using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Enums;

namespace CoursesApp.Domain.Interfaces.Repositories;

public interface IStudentRepository : IRepository<Student>
{
    Task<List<Student>> GetStudentsByGroupAsync(Guid groupId, CancellationToken ct = default);
    Task<(List<Student> Students, int TotalCount)> GetFilteredStudentAsync(
        string? searchQuery, Guid? groupId, int page, int pageSize, StudentSortKey sortKey, bool sortDesc, CancellationToken ct = default);
    Task<List<Guid>> DeleteAllByGroupAsync(Guid groupId, CancellationToken ct = default);
    Task<List<(string FirstName, string LastName)>> SuggestAsync(string query, Guid? groupId, int take, CancellationToken ct = default);
}
