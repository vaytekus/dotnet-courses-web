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

    public async Task UnassignTeacherAsync(Guid teacherId, CancellationToken ct = default)
    {
        var groups = await Context.Groups
            .Where(g => g.TeacherId == teacherId)
            .ToListAsync(ct);

        foreach (var group in groups)
        {
            group.TeacherId = null;
        }
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
}
