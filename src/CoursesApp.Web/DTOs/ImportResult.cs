namespace CoursesApp.Web.DTOs
{
    public class ImportResult
    {
        public int ImportedCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}