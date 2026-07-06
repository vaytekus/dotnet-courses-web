using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Interfaces;
using CoursesApp.Domain.Interfaces.Repositories;
using CoursesApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoursesApp.Infrastructure.Repositories
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly AppDbContext _context;

        public TeacherRepository(AppDbContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            _context = context;
        }
        
        public async Task<List<Teacher>> GetAllTeachersAsync(CancellationToken ct = default)
        {
            return await _context.Teachers.ToListAsync(ct);
        }

        public async Task<(List<Teacher>, int TotalCount)> GetFilteredTeachersAsync(string? search, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _context.Teachers
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
            return await _context.Teachers.FindAsync(id, ct);
        }
        
        public void AddTeacher(Teacher teacher)
        {
            _context.Teachers.Add(teacher); 
        }

        public void UpdateTeacher(Teacher teacher)
        {
            _context.Teachers.Update(teacher);
        }

        public void DeleteTeacher(Teacher teacher)
        {
            _context.Teachers.Remove(teacher);
        }
    }
}