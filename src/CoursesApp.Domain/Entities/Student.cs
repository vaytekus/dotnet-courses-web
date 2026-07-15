namespace CoursesApp.Domain.Entities;

public class Student
{
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public Guid GroupId { get; set; }
    public Group Group { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? TaxId { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? PostalCode { get; set; }
}