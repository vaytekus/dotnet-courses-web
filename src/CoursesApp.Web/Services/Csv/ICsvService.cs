namespace CoursesApp.Web.Services;

public interface ICsvService
{
    Task<byte[]> ExportGroupCsvAsync(Guid groupId, CancellationToken ct = default);
    Task<ImportResult> ImportGroupCsvAsync(Stream stream, Guid groupId, CancellationToken ct = default);
}
