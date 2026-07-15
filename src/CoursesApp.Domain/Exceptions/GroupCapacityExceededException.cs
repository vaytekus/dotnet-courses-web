namespace CoursesApp.Domain.Exceptions;

public class GroupCapacityExceededException(string message) : Exception(message);
