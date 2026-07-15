namespace CoursesApp.Web.Services;

public record StudentCsvRow(
    string FirstName,
    string LastName,
    DateOnly? DateOfBirth,
    Gender? Gender,
    string? TaxId,
    string? Email,
    string? Phone,
    string? City,
    string? Street,
    string? PostalCode);