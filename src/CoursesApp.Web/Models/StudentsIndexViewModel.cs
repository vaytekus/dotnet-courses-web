using CoursesApp.Web.DTOs;

namespace CoursesApp.Web.Models
{
    public class StudentsIndexViewModel
    {
        public List<StudentDto> Students { get; set; } = new();
        public List<GroupSelectDto> Groups { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}