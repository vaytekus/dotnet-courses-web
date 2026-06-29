namespace CoursesApp.Web.DTOs
{
    public class GroupEditDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public Guid? TeacherId { get; set; }
    }
}