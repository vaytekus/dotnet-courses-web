using CoursesApp.Domain.Exceptions;
using Microsoft.Extensions.Caching.Memory;

namespace CoursesApp.Web.Services;

public class GroupService(
    IUnitOfWork uow,
    IMemoryCache cache,
    ILogger<GroupService> logger) : IGroupService
{
    private static readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(5);
    private const string _coursesCacheKey = "courses_all";
    private const string _teachersCacheKey = "teachers_all";

    private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    private readonly ILogger<GroupService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IMemoryCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    
    public async Task<GroupsIndexViewModel> GetPageAsync(
        string? search, 
        Guid? courseId, 
        GroupStudentFilter filter, 
        int page, 
        int pageSize, 
        CancellationToken ct = default)
    {
        _logger.LogInformation("Loading groups page: search={Search}, courseId={CourseId}, filter={Filter}, page={Page}", search, courseId, filter, page);
        var (groups, total) = await _uow.Groups.GetFilteredGroupAsync(search, courseId, filter, page, pageSize, ct);

        var totalPages = total > 0 ? (int)Math.Ceiling((double)total / pageSize) : 1;
        if (page > totalPages)
        {
            page = totalPages;
            (groups, total) = await _uow.Groups.GetFilteredGroupAsync(search, courseId, filter, page, pageSize, ct);
        }

        if (!_cache.TryGetValue(_coursesCacheKey, out List<Course>? courses))
        {
            courses = await _uow.Courses.GetAllCoursesAsync(ct);
            _cache.Set(_coursesCacheKey, courses, _cacheTtl);
        }

        if (!_cache.TryGetValue(_teachersCacheKey, out List<Teacher>? teachers))
        {
            teachers = await _uow.Teachers.GetAllTeachersAsync(ct);
            _cache.Set(_teachersCacheKey, teachers, _cacheTtl);
        }

        return new GroupsIndexViewModel
        {
            Groups = groups.ToGroupDtoList(),
            Courses = courses!.ToDtoList(),
            Teachers = teachers!.ToDtoList(),
            Page = page,
            TotalCount = total,
            PageSize = pageSize
        };
    }

    public async Task<List<GroupSelectDto>> GetAllSelectAsync(CancellationToken ct = default)
    {
        var groups = await _uow.Groups.GetAllGroupAsync(ct);
        return groups.ToSelectDtoList();
    }

    public async Task AddGroupAsync(GroupCreateDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var name = dto.Name.Trim();
        
        if (await _uow.Groups.NameExistsAsync(name, null, ct))
        {
            throw new DuplicateNameException($"Group with name '{name}' already exists");
        }
        
        _logger.LogInformation("Adding group {Name} for course {CourseId}", name, dto.CourseId);
        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = name,
            CourseId = dto.CourseId,
            TeacherId = dto.TeacherId,
        };
        _uow.Groups.Add(group);
        await _uow.SaveAsync(ct);
        _logger.LogInformation("Group {Id} added successfully", group.Id);
        InvalidateCache();
    }

    public async Task UpdateGroupAsync(GroupEditDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var name = dto.Name.Trim();
        
        if (await _uow.Groups.NameExistsAsync(name, dto.Id, ct))
        {
            throw new DuplicateNameException($"Group with name '{name}' already exists");
        }
        
        _logger.LogInformation("Updating group {Id}", dto.Id);
        var group = await _uow.Groups.GetByIdAsync(dto.Id, ct)
                    ?? throw new KeyNotFoundException($"Group with id {dto.Id} not found");
        group.Name = name;
        group.TeacherId = dto.TeacherId;
        _uow.Groups.Update(group);
        await _uow.SaveAsync(ct);
        _logger.LogInformation("Group {Id} updated successfully", dto.Id);
        InvalidateCache();
    }

    public async Task UnassignTeacherAsync(Guid teacherId, CancellationToken ct = default)
    {
        await _uow.Groups.UnassignTeacherAsync(teacherId, ct);
    }

    public async Task<bool> DeleteGroupAsync(Guid id, bool deleteStudents = false, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting group {Id}", id);
        var group = await _uow.Groups.GetByIdAsync(id, ct)
                    ?? throw new KeyNotFoundException($"Group with id {id} not found");
        if (group.Students.Any())
        {
            if (!deleteStudents)
            {
                _logger.LogWarning("Cannot delete group {Id} — it has {Count} students", id, group.Students.Count);
                return false;
            }

            await _uow.Students.DeleteAllByGroupAsync(id, ct);
        }
        
        _uow.Groups.Delete(group);
        await _uow.SaveAsync(ct);
        _logger.LogInformation("Group {Id} deleted successfully", id);
        InvalidateCache();
        
        return true;
    }

    private void InvalidateCache()
    {
        _cache.Remove(_coursesCacheKey);
        _cache.Remove(_teachersCacheKey);
    }
}
