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

    public async Task<(List<Student>, int TotalCount)> GetFilteredStudentAsync(
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
            query = query.Where(s => 
                s.FirstName.Contains(searchQuery) ||
                s.LastName.Contains(searchQuery) || 
                s.Group.Name.Contains(searchQuery));
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
        var students = await Context.Students
            .Where(s => s.GroupId == groupId)
            .ToListAsync(ct);
        
        var ids = students.Select(s => s.Id).ToList();
        Context.Students.RemoveRange(students);
        return ids;
    }
}
