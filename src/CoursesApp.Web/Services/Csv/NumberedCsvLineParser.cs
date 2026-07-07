namespace CoursesApp.Web.Services;

public class NumberedCsvLineParser : CsvLineParserBase
{
    public override bool CanParse(string[] parts) =>
        parts.Length >= 3 && int.TryParse(parts[0].Trim(), out _);

    public override (string FirstName, string LastName) Parse(string[] parts) =>
        (Clean(parts[1]), Clean(parts[2]));
}
