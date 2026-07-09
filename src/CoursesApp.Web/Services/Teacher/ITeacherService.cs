namespace CoursesApp.Web.Services;

public interface ITeacherService
{
    Task<(List<TeacherDto> Teachers, int TotalCount)> GetPageAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<Guid> AddTeacherAsync(TeacherDto teacher, CancellationToken ct = default);
    Task UpdateTeacherAsync(TeacherEditDto dto, CancellationToken ct = default);
    Task ValidateExistAsync(Guid id, CancellationToken ct = default);
    Task DeleteTeacherAsync(Guid id, CancellationToken ct = default);
    
}
