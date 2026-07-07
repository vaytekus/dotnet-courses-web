namespace CoursesApp.Web.Services;

public class SimpleCsvLineParser : CsvLineParserBase
{
    public override bool CanParse(string[] parts) => parts.Length >= 2;

    public override (string FirstName, string LastName) Parse(string[] parts) =>
        (Clean(parts[0]), Clean(parts[1]));
}
