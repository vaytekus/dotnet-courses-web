using CoursesApp.Domain.Entities;
using CoursesApp.Web.DTOs;

namespace CoursesApp.Web.Mappers
{
    public static class TeacherMapper
    {
        public static TeacherDto ToDto(this Teacher groupDto) => new()
        {
            Id = groupDto.Id,
            FirstName = groupDto.FirstName,
            LastName = groupDto.LastName
        };

        public static List<TeacherDto> ToDtoList(this List<Teacher> teachers)
        {
            return teachers.Select(t => t.ToDto()).ToList();
        }
    }
}