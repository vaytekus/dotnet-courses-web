using CoursesApp.Web.DTOs;

namespace CoursesApp.Web.Models
{
    public class StudentsIndexViewModel
    {
        public List<StudentDto> Students { get; set; } = new();
        public List<GroupDto> Groups { get; set; } = new();
    }
}