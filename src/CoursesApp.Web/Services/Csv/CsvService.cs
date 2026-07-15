using System.Text;

namespace CoursesApp.Web.Services;

public class CsvService(
    IUnitOfWork uow,
    IEnumerable<ICsvLineParser> parsers) : ICsvService
{
    private const string _csvHeader = "#,First Name,Last Name,DOB,Gender,TaxId,Email,Phone,City,Street,PostalCode";
    private readonly IUnitOfWork _uow = uow;
    private readonly IEnumerable<ICsvLineParser> _parsers = parsers;

    public async Task<byte[]> ExportGroupCsvAsync(Guid groupId, CancellationToken ct = default)
    {
        var students = await _uow.Students.GetStudentsByGroupAsync(groupId, ct);
        var sb = new StringBuilder();
        sb.AppendLine(_csvHeader);

        var i = 1;
        foreach (var s in students)
        {
            sb.AppendLine(string.Join(",",
                (i++).ToString(),
                Escape(s.FirstName),
                Escape(s.LastName),
                s.DateOfBirth?.ToString("yyyy-MM-dd") ?? "",
                s.Gender?.ToString() ?? "",
                Escape(s.TaxId ?? ""),
                Escape(s.Email ?? ""),
                Escape(s.Phone ?? ""),
                Escape(s.City ?? ""),
                Escape(s.Street ?? ""),
                Escape(s.PostalCode ?? "")));
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
        var capacity = await _uow.Groups.GetCapacityAsync(groupId, ct);
        var remaining = capacity.Remaining;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = SplitCsvLine(line);
            var parser = _parsers.FirstOrDefault(p => p.CanParse(parts));

            if (parser is null)
            {
                result.Errors.Add($"Line {lineNumber} is invalid");
                continue;
            }

            var row = parser.Parse(parts);

            if (string.IsNullOrWhiteSpace(row.FirstName) || string.IsNullOrWhiteSpace(row.LastName))
            {
                result.Errors.Add($"Line {lineNumber}: first and last name are required!");
                continue;
            }

            if (remaining <= 0)
            {
                result.Errors.Add(
                    $"Line {lineNumber}: group is full (max {capacity.MaxCapacity}) — skipping");
                continue;
            }
            remaining--;

            _uow.Students.Add(new Domain.Entities.Student
            {
                Id = Guid.NewGuid(),
                FirstName = row.FirstName,
                LastName = row.LastName,
                GroupId = groupId,
                DateOfBirth = row.DateOfBirth,
                Gender = row.Gender,
                TaxId = row.TaxId,
                Email = row.Email,
                Phone = row.Phone,
                City = row.City,
                Street = row.Street,
                PostalCode = row.PostalCode
            });

            result.ImportedCount++;
        }

        if (result.ImportedCount > 0)
        {
            await _uow.SaveAsync(ct);
        }

        return result;
    }

    private static string[] SplitCsvLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }
        result.Add(sb.ToString());
        return result.ToArray();
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