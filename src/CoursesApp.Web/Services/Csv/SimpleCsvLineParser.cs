namespace CoursesApp.Web.Services;

public class SimpleCsvLineParser : CsvLineParserBase
{
    private const int _minParts = 2;

    public override bool CanParse(string[] parts) => parts.Length >= _minParts;

    public override StudentCsvRow Parse(string[] parts) => new(
        Clean(parts[0]),
        Clean(parts[1]),
        ParseDate(CleanOpt(parts, 2)),
        ParseGender(CleanOpt(parts, 3)),
        CleanOpt(parts, 4),
        CleanOpt(parts, 5),
        CleanOpt(parts, 6),
        CleanOpt(parts, 7),
        CleanOpt(parts, 8),
        CleanOpt(parts, 9));
}