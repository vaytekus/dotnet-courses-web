namespace CoursesApp.Web.Services;

public abstract class CsvLineParserBase : ICsvLineParser
{
    public abstract bool CanParse(string[] parts);
    public abstract StudentCsvRow Parse(string[] parts);

    protected static string Clean(string value) => value.Trim().Trim('"');

    protected static string? CleanOpt(string[] parts, int idx)
    {
        if (idx >= parts.Length) return null;
        var v = Clean(parts[idx]);
        return string.IsNullOrWhiteSpace(v) ? null : v;
    }

    protected static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParse(value, out var d) ? d : null;

    protected static Gender? ParseGender(string? value) =>
        Enum.TryParse<Gender>(value, true, out var g) ? g : null;
}