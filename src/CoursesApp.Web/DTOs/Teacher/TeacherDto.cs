namespace CoursesApp.Web.DTOs;

public record TeacherDto(
    Guid? Id,
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth = null,
    string? Email = null,
    string? Phone = null,
    DateOnly? HireDate = null,
    string? Degree = null);