namespace CoursesApp.Domain.Entities;

public class Teacher
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public ICollection<Group> Groups { get; set; } = [];
    
    public DateOnly? DateOfBirth { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateOnly? HireDate { get; set; }
    public string? Degree { get; set; }
}
