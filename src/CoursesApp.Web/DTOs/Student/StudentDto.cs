namespace CoursesApp.Web.DTOs;

public record StudentDto(
    Guid? Id,
    string FirstName,
    string LastName,
    Guid? GroupId,
    DateOnly? DateOfBirth = null,
    Gender? Gender = null,
    string? TaxId = null,
    string? Email = null,
    string? Phone = null,
    string? City = null,
    string? Street = null,
    string? PostalCode = null);