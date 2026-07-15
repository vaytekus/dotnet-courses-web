namespace CoursesApp.Web.Models;

public class GroupStudentsPageViewModel
{
    public required List<StudentDto> Students { get; init; }
    public required Guid GroupId { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public StudentSortKey SortKey { get; set; } = StudentSortKey.LastName;
    public bool SortDesc { get; set; } = false;
    public bool ShowActions { get; init; } = true;
}
