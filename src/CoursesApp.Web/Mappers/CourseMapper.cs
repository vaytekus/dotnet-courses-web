using CoursesApp.Domain.Entities;
using CoursesApp.Web.DTOs;

namespace CoursesApp.Web.Mappers
{
    public static class CourseMapper
    {
        public static CourseDto ToDto(this Course course) => new()
        {
            Id = course.Id,
            Name = course.Name,
        };
        
        public static List<CourseDto> ToDtoList(this List<Course> courses)
            => courses.Select(c => c.ToDto()).ToList();
    }
}