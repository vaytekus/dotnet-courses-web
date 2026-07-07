namespace CoursesApp.Web.Services;

public abstract class CsvLineParserBase : ICsvLineParser
{
    public abstract bool CanParse(string[] parts);
    public abstract (string FirstName, string LastName) Parse(string[] parts);

    protected static string Clean(string value) => value.Trim().Trim('"');
}