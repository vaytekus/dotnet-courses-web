namespace CoursesApp.Web.Services;

public class NumberedCsvLineParser : CsvLineParserBase
{
    private const int _minParts = 3;
    private const int _numberIndex = 0;
    private const int _firstNameIndex = 1;
    private const int _lastNameIndex = 2;

    public override bool CanParse(string[] parts) =>
        parts.Length >= _minParts && int.TryParse(parts[_numberIndex].Trim(), out _);

    public override (string FirstName, string LastName) Parse(string[] parts) =>
        (Clean(parts[_firstNameIndex]), Clean(parts[_lastNameIndex]));
}
