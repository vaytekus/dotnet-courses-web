namespace CoursesApp.Web.Models;

public class TeachersIndexViewModel
{
    public List<TeacherDto> Teachers { get; set; } = [];
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public TeacherSortKey SortKey { get; set; } = TeacherSortKey.LastName;
    public bool SortDesc { get; set; } = false;
}
