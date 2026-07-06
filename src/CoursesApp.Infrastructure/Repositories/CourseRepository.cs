using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Interfaces;
using CoursesApp.Domain.Interfaces.Repositories;
using CoursesApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoursesApp.Infrastructure.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly AppDbContext _context;

        public CourseRepository(AppDbContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            _context = context;
        }
        
        public async Task<List<Course>> GetAllCoursesAsync(CancellationToken ct = default)
        {
            return await _context.Courses
                .ToListAsync(ct);
        }

        public async Task<List<Course>> GetAllCoursesWithDetailsAsync(CancellationToken ct = default)
        {
            return await _context.Courses
                .Include(c => c.Groups)
                .AsSplitQuery()
                .ToListAsync(ct);
        }

        public async Task<List<Course>> SearchCoursesAsync(string query, CancellationToken ct = default)
        {
            return await _context.Courses
                .Include(c => c.Groups)
                .Where(c => c.Name.ToLower().Contains(query.ToLower()))
                .AsSplitQuery()
                .ToListAsync(ct);
        }
    }
}