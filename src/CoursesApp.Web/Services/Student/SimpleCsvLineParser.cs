namespace CoursesApp.Web.Services
{
    public class SimpleCsvLineParser : ICsvLineParser
    {
        public bool CanParse(string[] parts) => parts.Length >= 2;

        public (string FirstName, string LastName) Parse(string[] parts) =>
            (parts[0].Trim().Trim('"'), parts[1].Trim().Trim('"'));
    }
}