using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Enums;
using CoursesApp.Domain.Interfaces;
using CoursesApp.Domain.Interfaces.Repositories;
using CoursesApp.Infrastructure.Data;
using CoursesApp.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CoursesApp.Infrastructure.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly AppDbContext _context;

        public GroupRepository(AppDbContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            _context = context;
        }

        public async Task<List<Group>> GetAllGroupAsync(CancellationToken ct = default)
        {
            return await _context.Groups.ToListAsync(ct);
        }

        public async Task<(List<Group> Groups, int TotalCount)> GetFilteredGroupAsync(
            string? search, Guid? courseId, GroupStudentFilter studentFilter, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _context.Groups
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
            var groups = await _context.Groups
                .Where(g => g.TeacherId == teacherId)
                .ToListAsync(ct);

            foreach (var group in groups)
            {
                group.TeacherId = null;
            }
        }

        public Task<Group?> GetByIdAsync(Guid groupId, CancellationToken ct = default)
        {
            return _context.Groups
                .Include(g => g.Students)
                .FirstOrDefaultAsync(g => g.Id == groupId, ct);
        }
        
        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void UpdateGroup(Group group)
        {
            _context.Groups.Update(group);
        }

        public void DeleteGroup(Group group)
        {
            _context.Groups.Remove(group);
        }
    }
}