using CoursesApp.Domain.Exceptions;

namespace CoursesApp.Web.Services;

public class StudentService(
    IUnitOfWork uow,
    ILogger<StudentService> logger) : IStudentService
{
    private readonly IUnitOfWork _uow = uow;
    private readonly ILogger<StudentService> _logger = logger;

    public async Task<(List<StudentDto> Students, int TotalCount)> GetPageAsync(
        string? search, Guid? id, StudentSortKey sortKey, bool sortDesc, int page, int pageSize, CancellationToken ct = default)
    {
        _logger.LogInformation("Getting page {Page} of {PageSize}, sort={SortKey} desc={SortDesc}", page, pageSize, sortKey, sortDesc);
        var (students, total) = await _uow.Students.GetFilteredStudentAsync(search, id, page, pageSize, sortKey, sortDesc, ct);
        return (students.ToDtoList(), total);
    }

    public async Task<List<StudentDto>> GetStudentsByGroupAsync(Guid groupId, CancellationToken ct = default)
    {
        _logger.LogInformation("Loading students for group {GroupId}", groupId);
        var students = await _uow.Students.GetStudentsByGroupAsync(groupId, ct);
        return students.ToDtoList();
    }

    public async Task AddStudentAsync(StudentDto studentDto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(studentDto);
        var groupId = studentDto.GroupId ?? Guid.Empty;

        var capacity = await _uow.Groups.GetCapacityAsync(groupId, ct);
        if (capacity.IsFull)
        {
            throw new GroupCapacityExceededException(
                $"Group is full — max capacity is {capacity.MaxCapacity} students");
        }
        
        _logger.LogInformation("Adding student {FirstName} {LastName} to group {GroupId}",
            studentDto.FirstName, studentDto.LastName, studentDto.GroupId);
        var student = new Domain.Entities.Student
        {
            Id = Guid.NewGuid(),
            FirstName = studentDto.FirstName,
            LastName = studentDto.LastName,
            GroupId = studentDto.GroupId ?? Guid.Empty
        };
        _uow.Students.Add(student);
        await _uow.SaveAsync(ct);
        _logger.LogInformation("Student {Id} added successfully", student.Id);
    }

    public async Task<Guid> UpdateStudentAsync(StudentEditDto studentDto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(studentDto);
        _logger.LogInformation("Updating student {Id}", studentDto.Id);
        var student = await _uow.Students.GetByIdAsync(studentDto.Id, ct)
            ?? throw new KeyNotFoundException($"Student {studentDto.Id} not found");

        var oldGroupId = student.GroupId;
        if (oldGroupId != studentDto.GroupId)
        {
            var capacity = await _uow.Groups.GetCapacityAsync(studentDto.GroupId, ct);
            if (capacity.IsFull)
            {
                throw new GroupCapacityExceededException(
                    $"Target group is full — max capacity is {capacity.MaxCapacity} students");
            }
        }
        
        student.FirstName = studentDto.FirstName;
        student.LastName = studentDto.LastName;
        student.GroupId = studentDto.GroupId;
        _uow.Students.Update(student);
        await _uow.SaveAsync(ct);
        _logger.LogInformation("Student {Id} updated successfully", studentDto.Id);

        return oldGroupId;
    }

    public async Task DeleteStudentAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting student {Id}", id);
        var student = await _uow.Students.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Student {id} not found");
        _uow.Students.Delete(student);
        await _uow.SaveAsync(ct);
        _logger.LogInformation("Student {Id} deleted successfully", id);
    }

    public async Task<List<Guid>> ClearAllStudentsAsync(Guid id, CancellationToken ct = default)
    {
        _logger.LogInformation("Clearing all students from group {GroupId}", id);
        var ids = await _uow.Students.DeleteAllByGroupAsync(id, ct);
        await _uow.SaveAsync(ct);
        _logger.LogInformation("All students cleared from group {GroupId}", id);
        return ids;
    }
    public async Task<List<StudentSuggestionDto>> SuggestAsync(
        string query, Guid? groupId, int take, CancellationToken ct = default)
    {
        _logger.LogInformation("Suggesting students for '{Query}' groupId={GroupId} take={Take}", query, groupId, take);
        var rows = await _uow.Students.SuggestAsync(query, groupId, take, ct);

        return rows.Select(r => new StudentSuggestionDto(r.FirstName, r.LastName)).ToList();
    }
}
