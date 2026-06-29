namespace CoursesApp.Web.DTOs
{
    public class StudentEditDto
    {
        public Guid Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public Guid GroupId { get; set; }
    }
}