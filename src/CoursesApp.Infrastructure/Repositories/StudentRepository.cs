using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Interfaces;
using CoursesApp.Domain.Interfaces.Repositories;
using CoursesApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoursesApp.Infrastructure.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _context;

        public StudentRepository(AppDbContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            _context = context;
        }

        public async Task<List<Student>> GetStudentsByGroupAsync(Guid groupId, CancellationToken ct = default)
        {
            return await _context.Students
                .Where(s => s.GroupId == groupId)
                .ToListAsync(ct);
        }

        public async Task<(List<Student>, int TotalCount)> GetFilteredStudentAsync(
            string? searchQuery, Guid? groupId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _context.Students
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
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
            
            return (students, total);
        }
        
        public async Task<Student?> GetStudentByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Students.FindAsync(id, ct);
        }
        
        public void AddStudent(Student student)
        {
            _context.Students.Add(student);
        }

        public void UpdateStudent(Student student)
        {
            _context.Students.Update(student);
        }

        public void DeleteStudent(Student student)
        {
            _context.Students.Remove(student);
        }

        public async Task DeleteAllByGroupAsync(Guid groupId, CancellationToken ct = default)
        {
            var students = await _context.Students
                .Where(s => s.GroupId == groupId)
                .ToListAsync(ct);
            
            _context.Students.RemoveRange(students);
        }
    }
}