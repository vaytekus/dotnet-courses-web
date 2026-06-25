using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Enums;

namespace CoursesApp.Domain.Interfaces
{
    public interface IGroupRepository
    {
        Task<List<Group>> GetAllGroupAsync();
        Task<List<Group>> GetAllGroupWithDetailsAsync();
        Task<List<Group>> GetFilteredGroupAsync(string query, Guid? courseID, GroupStudentFilter studentFilter, int page, int pageSize);
        Task<Group?> GetByIdAsync(Guid groupId);
        Task<int> GetGroupsCountAsync(string searchQuery, Guid? courseID, GroupStudentFilter studentFilter);
        void AddGroup(Group group);
        void UpdateGroup(Group group);
        void DeleteGroup(Group group);
    }
}