using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Enums;

namespace CoursesApp.Domain.Interfaces.Repositories;

public interface ITeacherRepository: IRepository<Teacher>
{
    Task<List<Teacher>> GetAllTeachersAsync(CancellationToken ct = default);
    Task<(List<Teacher>, int TotalCount)> GetFilteredTeachersAsync(
        string? search, TeacherSortKey sortKey, bool sortDesc, int page, int pageSize, CancellationToken ct = default);
    Task<List<(string FirstName, string LastName)>> SuggestAsync(string query, int take, CancellationToken ct = default); 
}
