namespace CoursesApp.Web.DTOs.Auth;

public record RegisterDto(string Email,  string Password, string ConfirmPassword);
