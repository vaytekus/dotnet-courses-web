using CoursesApp.Web.DTOs;

namespace CoursesApp.Web.Services
{
    public interface IStudentCsvService
    {
        Task<byte[]> ExportGroupCsvAsync(Guid groupId, CancellationToken ct = default);
        Task<ImportResult> ImportGroupCsvAsync(Stream stream, Guid groupId, CancellationToken ct = default);
    }
}