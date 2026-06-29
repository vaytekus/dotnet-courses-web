namespace CoursesApp.Web.DTOs
{
    public class GroupCreateDto
    {
        public required string Name { get; set; }
        public Guid CourseId { get; set; }
        public Guid? TeacherId { get; set; }
    }
}