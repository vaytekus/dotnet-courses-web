namespace CoursesApp.Web.Services
{
    public interface ICsvLineParser
    {
        bool CanParse(string[] parts);
        (string FirstName, string LastName) Parse(string[] parts);
    }
}