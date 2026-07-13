namespace CoursesApp.Web.Services;

public class SimpleCsvLineParser : CsvLineParserBase
{
    private const int _minParts = 2;
    private const int _firstNameIndex = 0;
    private const int _lastNameIndex = 1;

    public override bool CanParse(string[] parts) => parts.Length >= _minParts;

    public override (string FirstName, string LastName) Parse(string[] parts) =>
        (Clean(parts[_firstNameIndex]), Clean(parts[_lastNameIndex]));
}
