namespace CoursesApp.Web.Services;

public interface IStudentService
{
    Task<(List<StudentDto> Students, int TotalCount)> GetPageAsync(
        string? search, Guid? id, StudentSortKey sortKey, bool sortDesc, int page, int pageSize, CancellationToken ct = default);
    Task<List<StudentDto>> GetStudentsByGroupAsync(Guid groupId, CancellationToken ct = default);
    Task AddStudentAsync(StudentDto studentDto, CancellationToken ct = default);
    Task<Guid> UpdateStudentAsync(StudentEditDto dto, CancellationToken ct = default);
    Task DeleteStudentAsync(Guid id, CancellationToken ct = default);
    Task<List<Guid>> ClearAllStudentsAsync(Guid id, CancellationToken ct = default);
    Task<List<StudentSuggestionDto>> SuggestAsync(string query, Guid? groupId, int take, CancellationToken ct = default);
}
