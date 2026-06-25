using CoursesApp.Domain.Entities;
using CoursesApp.Web.DTOs;

namespace CoursesApp.Web.Mappers
{
    public static class StudentMapper
    {
        public static StudentDto ToDto(this Student student) => new()
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            GroupId = student.GroupId
        };

        public static List<StudentDto> ToDtoList(this List<Student> students)
        {
            return students.Select(s => s.ToDto()).ToList();
        } 
    }
}