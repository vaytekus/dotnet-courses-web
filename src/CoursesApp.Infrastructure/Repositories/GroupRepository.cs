using CoursesApp.Infrastructure.Extensions;

namespace CoursesApp.Infrastructure.Repositories;

public class GroupRepository(AppDbContext context) : RepositoryBase(context), IGroupRepository
{
    public async Task<List<Group>> GetAllGroupAsync(CancellationToken ct = default)
    {
        return await Context.Groups.ToListAsync(ct);
    }

    public async Task<(List<Group> Groups, int TotalCount)> GetFilteredGroupAsync(
        string? search, Guid? courseId, GroupStudentFilter studentFilter, int page, int pageSize, CancellationToken ct = default)
    {
        var query = Context.Groups
            .Include(g => g.Course)
            .Include(g => g.Teacher)
            .Include(g => g.Students)
            .AsSplitQuery()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(g => g.Name.Contains(search));
        }

        if (courseId.HasValue)
        {
            query = query.Where(g => g.CourseId == courseId.Value);
        }

        query = studentFilter.Apply(query);
        
        var total = await query.CountAsync(ct);

        var groups = await query
            .OrderBy(g => g.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (groups, total);
    }

    public Task UnassignTeacherAsync(Guid teacherId, CancellationToken ct = default)
    {
        return Context.Groups
            .Where(g => g.TeacherId == teacherId)
            .ExecuteUpdateAsync(
            setters => setters.SetProperty(g => g.TeacherId, (Guid?)null),
            ct);
    }

    public Task<Group?> GetByIdAsync(Guid groupId, CancellationToken ct = default)
    {
        return Context.Groups
            .Include(g => g.Students)
            .FirstOrDefaultAsync(g => g.Id == groupId, ct);
    }
    
    public void Add(Group group)
    {
        Context.Groups.Add(group);
    }

    public void Update(Group group)
    {
        Context.Groups.Update(group);
    }

    public void Delete(Group group)
    {
        Context.Groups.Remove(group);
    }

    public Task<bool> NameExistsAsync(string name, Guid? excludeId, CancellationToken ct = default)
    {
        var query = Context.Groups
            .Where(g => g.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(g => g.Id != excludeId);
        }

        return query.AnyAsync(ct);
    }

    public async Task<GroupCapacityInfo> GetCapacityAsync(Guid groupId, CancellationToken ct = default)
    {
        var info = await Context.Groups
            .Where(g => g.Id == groupId)
            .Select(g => new GroupCapacityInfo(g.MaxCapacity, g.Students.Count))
            .FirstOrDefaultAsync(ct);

        return info ?? new GroupCapacityInfo(null, 0);
    }

    public async Task<List<string>> SuggestAsync(string query, int take, CancellationToken ct = default)
    {
        return await Context.Groups
            .Where(g => g.Name.Contains(query))
            .Select(g => g.Name)
            .Distinct()
            .OrderBy(n => n)
            .Take(take)
            .ToListAsync(ct);
    }
}
