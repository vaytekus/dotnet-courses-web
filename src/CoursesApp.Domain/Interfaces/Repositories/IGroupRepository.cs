using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Enums;

namespace CoursesApp.Domain.Interfaces
{
    public interface IGroupRepository
    {
        Task<List<Group>> GetAllGroupAsync();
        Task<(List<Group> Groups, int TotalCount)> GetFilteredGroupAsync(
            string? search, 
            Guid? courseID, 
            GroupStudentFilter studentFilter, 
            int page, 
            int pageSize);
        Task<Group?> GetByIdAsync(Guid groupId);
        void AddGroup(Group group);
        void UpdateGroup(Group group);
        void DeleteGroup(Group group);
    }
}