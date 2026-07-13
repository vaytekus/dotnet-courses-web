namespace CoursesApp.Web.Services;

public record GroupDeletionResult(bool Success, IReadOnlyList<Guid> DeletedStudentsIds);
