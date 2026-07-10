namespace CoursesApp.Web.Services;

public interface IGroupService
{
    Task<GroupsIndexViewModel> GetPageAsync(string? search, Guid? id, GroupStudentFilter filter, int page, int pageSize, CancellationToken ct = default);
    Task<List<GroupSelectDto>> GetAllSelectAsync(CancellationToken ct = default);
    Task AddGroupAsync(GroupCreateDto dto, CancellationToken ct = default);
    Task UpdateGroupAsync(GroupEditDto dto, CancellationToken ct = default);
    Task UnassignTeacherAsync(Guid teacherId, CancellationToken ct = default);
    Task<bool> DeleteGroupAsync(Guid id, bool deleteStudents = false, CancellationToken ct = default);
}
