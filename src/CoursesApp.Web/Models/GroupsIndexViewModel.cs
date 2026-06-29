using CoursesApp.Domain.Entities;
using CoursesApp.Web.DTOs;

namespace CoursesApp.Web.Models
{
    public class GroupsIndexViewModel
    {
        public List<GroupDto> Groups { get; set; } = new();
        public List<CourseDto> Courses { get; set; } = new();
        public List<TeacherDto> Teachers { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}