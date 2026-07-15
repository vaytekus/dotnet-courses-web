namespace CoursesApp.Domain.Entities;

public class Course
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public ICollection<Group> Groups { get; set; } = [];
    
    public CourseLevel? Level { get; set; }
    public int? DurationWeeks { get; set; }
    public string? Category { get; set; }
}
