namespace CoursesApp.Web.DTOs;

public record GroupCreateDto(string Name, Guid CourseId, Guid? TeacherId);
