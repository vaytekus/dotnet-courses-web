namespace CoursesApp.Web.DTOs;

public record StudentDto(Guid? Id, string FirstName, string LastName, Guid? GroupId);
