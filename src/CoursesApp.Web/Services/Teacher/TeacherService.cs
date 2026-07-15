namespace CoursesApp.Web.Services;

public class TeacherService(
    IUnitOfWork uow,
    ILogger<TeacherService> logger) : ITeacherService
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ILogger<TeacherService> _logger = logger;

    public async Task<(List<TeacherDto> Teachers, int TotalCount)> GetPageAsync(
        string? search, TeacherSortKey sortKey, bool sortDesc, 
        int page, int pageSize, CancellationToken ct = default)
    {
        var (teachers, total) = await _uow.Teachers.GetFilteredTeachersAsync(search, sortKey, sortDesc, page, pageSize, ct);
        return (teachers.ToDtoList(), total);
    }

    public async Task<Guid> AddTeacherAsync(TeacherDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        _logger.LogInformation("Adding teacher {FirstName} {LastName}", dto.FirstName, dto.LastName);
        var teacher = new Domain.Entities.Teacher
        {
            Id = Guid.NewGuid(),
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };
        _uow.Teachers.Add(teacher);
        await _uow.SaveAsync(ct);
        _logger.LogInformation("Teacher {Id} added successfully", teacher.Id);

        return teacher.Id;
    }

    public async Task UpdateTeacherAsync(TeacherEditDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        _logger.LogInformation("Updating teacher {Id}", dto.Id);
        var teacher = await _uow.Teachers.GetByIdAsync(dto.Id, ct)
            ?? throw new KeyNotFoundException($"Teacher {dto.Id} not found");
        teacher.FirstName = dto.FirstName;
        teacher.LastName = dto.LastName;
        _uow.Teachers.Update(teacher);
        await _uow.SaveAsync(ct);
        _logger.LogInformation("Teacher {Id} updated successfully", dto.Id);
    }

    public async Task ValidateExistAsync(Guid id, CancellationToken ct = default)
    {
        if (await _uow.Teachers.GetByIdAsync(id, ct) is null)
        {
            throw new KeyNotFoundException($"Teacher {id} not found");
        }
    }

    public async Task DeleteTeacherAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting teacher {Id}", id);
        var teacher = await _uow.Teachers.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Teacher {id} not found");
        _uow.Teachers.Delete(teacher);
        await _uow.SaveAsync(ct);
        _logger.LogInformation("Teacher {Id} deleted successfully", id);
    }
    public async Task<List<TeacherSuggestionDto>> SuggestAsync(string query, int take, CancellationToken ct = default)
    {
        _logger.LogInformation("Suggesting teachers for '{Query}' take={Take}", query, take);
        var rows = await _uow.Teachers.SuggestAsync(query, take, ct);
        return rows.Select(r => new TeacherSuggestionDto(r.FirstName, r.LastName)).ToList(); 
    }
}
