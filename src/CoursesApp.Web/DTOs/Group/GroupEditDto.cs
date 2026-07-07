namespace CoursesApp.Web.DTOs;

public record GroupEditDto(Guid Id, string Name, Guid? TeacherId);
