namespace CoursesApp.Infrastructure.Repositories;

public class CourseRepository(AppDbContext context) : RepositoryBase(context), ICourseRepository
{   
    public async Task<List<Course>> GetAllCoursesAsync(CancellationToken ct = default)
    {
        return await Context.Courses
            .ToListAsync(ct);
    }

    public async Task<List<Course>> GetAllCoursesWithDetailsAsync(CancellationToken ct = default)
    {
        return await Context.Courses
            .Include(c => c.Groups)
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    public async Task<List<Course>> SearchCoursesAsync(string query, CancellationToken ct = default)
    {
        return await Context.Courses
            .Include(c => c.Groups)
            .Where(c => c.Name.ToLower().Contains(query.ToLower()))
            .AsSplitQuery()
            .ToListAsync(ct);
    }
}
