namespace CoursesApp.Web.Services;

public interface ICsvLineParser
{
    bool CanParse(string[] parts);
    StudentCsvRow Parse(string[] parts);
}