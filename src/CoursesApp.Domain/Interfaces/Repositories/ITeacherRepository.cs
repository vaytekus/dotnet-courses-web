using CoursesApp.Domain.Entities;

namespace CoursesApp.Domain.Interfaces.Repositories;

public interface ITeacherRepository: IRepository<Teacher>
{
    Task<List<Teacher>> GetAllTeachersAsync(CancellationToken ct = default);
    Task<(List<Teacher>, int TotalCount)> GetFilteredTeachersAsync(string? search, int page, int pageSize, CancellationToken ct = default);
}
