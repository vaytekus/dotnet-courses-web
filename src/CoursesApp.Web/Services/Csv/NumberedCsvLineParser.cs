namespace CoursesApp.Web.Services;

public class NumberedCsvLineParser : CsvLineParserBase
{
    private const int _minParts = 3;

    public override bool CanParse(string[] parts) =>
        parts.Length >= _minParts && int.TryParse(Clean(parts[0]), out _);

    public override StudentCsvRow Parse(string[] parts) => new(
        Clean(parts[1]),
        Clean(parts[2]),
        ParseDate(CleanOpt(parts, 3)),
        ParseGender(CleanOpt(parts, 4)),
        CleanOpt(parts, 5),
        CleanOpt(parts, 6),
        CleanOpt(parts, 7),
        CleanOpt(parts, 8),
        CleanOpt(parts, 9),
        CleanOpt(parts, 10));
}