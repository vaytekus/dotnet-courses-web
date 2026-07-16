using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Enums;

namespace CoursesApp.Domain.Interfaces.Repositories;

public interface IGroupRepository : IRepository<Group>
{
    Task<List<Group>> GetAllGroupAsync(CancellationToken ct = default);
    Task<(List<Group> Groups, int TotalCount)> GetFilteredGroupAsync(
        string? search, 
        Guid? courseId, 
        GroupStudentFilter studentFilter, 
        int page, 
        int pageSize,
        CancellationToken ct = default);

    Task UnassignTeacherAsync(Guid teacherId, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeId, CancellationToken ct = default);
    Task<GroupCapacityInfo> GetCapacityAsync(Guid groupId, CancellationToken ct = default);
    Task<List<string>> SuggestAsync(string query, int take, CancellationToken ct = default);
}
