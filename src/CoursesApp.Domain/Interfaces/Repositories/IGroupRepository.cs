using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Enums;

namespace CoursesApp.Domain.Interfaces.Repositories;

public interface IGroupRepository
{
    Task<List<Group>> GetAllGroupAsync(CancellationToken ct = default);
    Task<(List<Group> Groups, int TotalCount)> GetFilteredGroupAsync(
        string? search, 
        Guid? courseID, 
        GroupStudentFilter studentFilter, 
        int page, 
        int pageSize,
        CancellationToken ct = default);

    Task UnassignTeacherAsync(Guid teacherId, CancellationToken ct = default);
    Task<Group?> GetByIdAsync(Guid groupId, CancellationToken ct = default);
    void AddGroup(Group group);
    void UpdateGroup(Group group);
    void DeleteGroup(Group group);
}
