using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Enums;
using CoursesApp.Domain.Interfaces;
using CoursesApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoursesApp.Infrastructure.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly AppDbContext _context;

        public GroupRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Group>> GetAllGroupAsync()
        {
            return await _context.Groups.ToListAsync();
        }

        public async Task<List<Group>> GetAllGroupWithDetailsAsync()
        {
            return await _context.Groups
                .Include(g => g.Course)
                .Include(g => g.Teacher)
                .Include(g => g.Students)
                .AsSplitQuery()
                .ToListAsync();
        }

        public Task<List<Group>> GetFilteredGroupAsync(string query, Guid? courseID, GroupStudentFilter studentFilter, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<Group?> GetByIdAsync(Guid groupId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetGroupsCountAsync(string searchQuery, Guid? courseID, GroupStudentFilter studentFilter)
        {
            throw new NotImplementedException();
        }
        
        public void AddGroup(Group group)
        {
            throw new NotImplementedException();
        }

        public void UpdateGroup(Group group)
        {
            throw new NotImplementedException();
        }

        public void DeleteGroup(Group group)
        {
            throw new NotImplementedException();
        }
    }
}