namespace CoursesApp.Web.DTOs
{
    public class StudentEditDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Guid GroupId { get; set; }
    }
}