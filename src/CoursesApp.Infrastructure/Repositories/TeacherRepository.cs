using CoursesApp.Infrastructure.Extensions;

namespace CoursesApp.Infrastructure.Repositories;

public class TeacherRepository(AppDbContext context) : RepositoryBase(context), ITeacherRepository
{
    public async Task<List<Teacher>> GetAllTeachersAsync(CancellationToken ct = default)
    {
        return await Context.Teachers.ToListAsync(ct);
    }

    public async Task<(List<Teacher>, int TotalCount)> GetFilteredTeachersAsync(
        string? search, TeacherSortKey sortKey, bool sortDesc, 
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = Context.Teachers
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            var tokens = search.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var token in tokens)
            {
                query = query.Where(s => s.FirstName.Contains(token) || 
                                     s.LastName.Contains(token));
            }
        }
        
        var total = await query.CountAsync(ct);
        var teachers = await query
            .ApplySort(sortKey, sortDesc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (teachers, total);
    }
    public async Task<List<(string FirstName, string LastName)>> SuggestAsync(
        string query, int take, CancellationToken ct = default)
    { 
        var q = Context.Teachers.AsQueryable();
        
        var rows = await q
            .Where(t => t.FirstName.Contains(query) || t.LastName.Contains(query))
            .Select(t => new {t.FirstName, t.LastName})
            .Distinct()
            .OrderBy(t => t.LastName).ThenBy(t => t.FirstName)
            .Take(take)
            .ToListAsync(ct);
        
        return rows.Select(r => (r.FirstName, r.LastName)).ToList();
    }

    public async Task<Teacher?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await Context.Teachers.FindAsync(id, ct);
    }
    
    public void Add(Teacher teacher)
    {
        Context.Teachers.Add(teacher); 
    }

    public void Update(Teacher teacher)
    {
        Context.Teachers.Update(teacher);
    }

    public void Delete(Teacher teacher)
    {
        Context.Teachers.Remove(teacher);
    }
}
