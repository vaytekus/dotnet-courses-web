using CoursesApp.Infrastructure.Extensions;

namespace CoursesApp.Infrastructure.Repositories;

public class StudentRepository(AppDbContext context) : RepositoryBase(context), IStudentRepository
{
    public async Task<List<Student>> GetStudentsByGroupAsync(Guid groupId, CancellationToken ct = default)
    {
        return await Context.Students
            .Where(s => s.GroupId == groupId)
            .ToListAsync(ct);
    }

    public async Task<(List<Student> Students, int TotalCount)> GetFilteredStudentAsync(
        string? searchQuery, 
        Guid? groupId, 
        int page, 
        int pageSize, 
        StudentSortKey sortKey,
        bool sortDesc,
        CancellationToken ct = default)
    {
        var query = Context.Students
            .Include(s => s.Group)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchQuery))
        {
            var tokens = searchQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                query = query.Where(s => 
                    s.FirstName.Contains(token) ||
                    s.LastName.Contains(token) || 
                    s.Group.Name.Contains(token));
            }
        }

        if (groupId.HasValue)
        {
            query = query.Where(s => s.GroupId == groupId);
        }
        
        var total = await query.CountAsync(ct);
        var students = await query
            .ApplySort(sortKey, sortDesc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (students, total);
    }
    
    public async Task<Student?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await Context.Students.FindAsync(id, ct);
    }
    
    public void Add(Student student)
    {
        Context.Students.Add(student);
    }

    public void Update(Student student)
    {
        Context.Students.Update(student);
    }

    public void Delete(Student student)
    {
        Context.Students.Remove(student);
    }

    public async Task<List<Guid>> DeleteAllByGroupAsync(Guid groupId, CancellationToken ct = default)
    {
        var ids = await Context.Students
            .Where(s => s.GroupId == groupId)
            .Select(s => s.Id)
            .ToListAsync(ct);
        
        if (ids.Count == 0)
        {
            return ids;
        }

        await Context.Students
            .Where(s => s.GroupId == groupId)
            .ExecuteDeleteAsync(ct);

        foreach (var entry in Context.ChangeTracker.Entries<Student>()
                     .Where(e => e.Entity.GroupId == groupId).ToList())
        {
            entry.State = EntityState.Detached;
        }
        
        return ids;
    }
    
    public async Task<List<(string FirstName, string LastName)>> SuggestAsync(
        string query, Guid? groupId, int take,
        CancellationToken ct = default)
    {
        var q = Context.Students.AsQueryable();

        if (groupId.HasValue)
        {
            q = q.Where(s => s.GroupId == groupId);
        }

        var rows = await q
            .Where(s => s.FirstName.Contains(query) || s.LastName.Contains(query))
            .Select(s => new { s.FirstName, s.LastName })
            .Distinct()
            .OrderBy(s => s.LastName).ThenBy(s => s.FirstName)
            .Take(take)
            .ToListAsync(ct);

        return rows.Select(r => (r.FirstName, r.LastName)).ToList();
    }
}
