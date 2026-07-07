namespace CoursesApp.Domain.Entities;

public class Teacher
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public ICollection<Group> Groups { get; set; } = [];
}
