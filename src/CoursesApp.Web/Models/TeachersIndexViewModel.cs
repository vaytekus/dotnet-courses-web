using CoursesApp.Web.DTOs;

namespace CoursesApp.Web.Models
{
    public class TeachersIndexViewModel
    {
        public List<TeacherDto> Teachers { get; set; } = new();
    }
}