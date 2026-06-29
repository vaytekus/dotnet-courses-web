using CoursesApp.Web.DTOs;

namespace CoursesApp.Web.Services
{
    public interface IGroupService
    {
        Task AddGroupAsync(GroupCreateDto dto);
        Task UpdateGroupAsync(GroupEditDto dto);
        Task<bool> DeleteGroupAsync(Guid id);
        Task ClearStudentAsync(Guid id);
    }
}