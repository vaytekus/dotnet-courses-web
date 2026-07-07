using System.Text;

namespace CoursesApp.Web.Services;

public class CsvService(
    IUnitOfWork uow,
    IEnumerable<ICsvLineParser> parsers) : ICsvService
{
    private const string _csvHeader = "#,First Name,Last Name";
    private readonly IUnitOfWork _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    private readonly IEnumerable<ICsvLineParser> _parsers = parsers ?? throw new ArgumentNullException(nameof(parsers));

    public async Task<byte[]> ExportGroupCsvAsync(Guid groupId, CancellationToken ct = default)
    {
        var students = await _uow.Students.GetStudentsByGroupAsync(groupId, ct);
        var sb = new StringBuilder();
        sb.AppendLine(_csvHeader);

        var i = 1;
        foreach (var student in students)
        {
            sb.AppendLine($"{i++},{Escape(student.FirstName)},{Escape(student.LastName)}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<ImportResult> ImportGroupCsvAsync(Stream stream, Guid groupId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var result = new ImportResult();
        using var reader = new StreamReader(stream);
        await reader.ReadLineAsync();
        var lineNumber = 1;
        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split(',');
            var parser = _parsers.FirstOrDefault(p => p.CanParse(parts));

            if (parser is null)
            {
                result.Errors.Add($"Line {lineNumber} is invalid");
                continue;
            }

            var (firstName, lastName) = parser.Parse(parts);

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                result.Errors.Add($"Line {lineNumber}: first and last name are required!");
                continue;
            }

            _uow.Students.AddStudent(new Domain.Entities.Student
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                GroupId = groupId
            });

            result.ImportedCount++;
        }

        if (result.ImportedCount > 0)
        {
            await _uow.SaveAsync(ct);
        }

        return result;
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
