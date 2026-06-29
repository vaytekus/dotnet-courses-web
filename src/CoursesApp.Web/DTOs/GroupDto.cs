using CoursesApp.Domain.Entities;

namespace CoursesApp.Web.DTOs
{
    public class GroupDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public Guid CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public Guid? TeacherId { get; set; }
        public Teacher? Teacher { get; set; } = null!;
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}