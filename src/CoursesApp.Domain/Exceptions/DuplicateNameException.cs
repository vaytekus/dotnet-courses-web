namespace CoursesApp.Domain.Exceptions;

public class DuplicateNameException(string message) : Exception(message);
