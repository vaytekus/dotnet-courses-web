namespace CoursesApp.Web.DTOs.Auth;

public record LoginDto(string Email, string Password, bool RememberMe);
