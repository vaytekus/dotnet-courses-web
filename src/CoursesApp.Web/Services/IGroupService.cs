using CoursesApp.Domain.Enums;
using CoursesApp.Web.DTOs;
using CoursesApp.Web.Models;

namespace CoursesApp.Web.Services
{
    public interface IGroupService
    {
        Task<GroupsIndexViewModel> GetPageAsync(string? search, Guid? id, GroupStudentFilter filter, int page, int pageSize);
        Task AddGroupAsync(GroupCreateDto dto);
        Task UpdateGroupAsync(GroupEditDto dto);
        Task<bool> DeleteGroupAsync(Guid id);
    }
}