namespace CoursesApp.Web.Services
{
    public class NumberedCsvLineParser : ICsvLineParser
    {
        public bool CanParse(string[] parts) =>
            parts.Length >= 3 && int.TryParse(parts[0].Trim(), out _);

        public (string FirstName, string LastName) Parse(string[] parts) =>
            (parts[1].Trim().Trim('"'), parts[2].Trim().Trim('"'));
    }
}