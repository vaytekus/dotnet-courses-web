namespace CoursesApp.Infrastructure.Repositories;

public class TeacherRepository(AppDbContext context) : RepositoryBase(context), ITeacherRepository
{
    public async Task<List<Teacher>> GetAllTeachersAsync(CancellationToken ct = default)
    {
        return await Context.Teachers.ToListAsync(ct);
    }

    public async Task<(List<Teacher>, int TotalCount)> GetFilteredTeachersAsync(string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var query = Context.Teachers
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s => s.FirstName.Contains(search) || 
                                     s.LastName.Contains(search));
        }
        
        var total = await query.CountAsync(ct);
        var teachers = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (teachers, total);
    }

    public async Task<Teacher?> GetTeacherByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await Context.Teachers.FindAsync(id, ct);
    }
    
    public void AddTeacher(Teacher teacher)
    {
        Context.Teachers.Add(teacher); 
    }

    public void UpdateTeacher(Teacher teacher)
    {
        Context.Teachers.Update(teacher);
    }

    public void DeleteTeacher(Teacher teacher)
    {
        Context.Teachers.Remove(teacher);
    }
}
