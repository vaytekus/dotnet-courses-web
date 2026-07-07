namespace CoursesApp.Infrastructure.Repositories;

public class StudentFilterOption
{
    public GroupStudentFilter Value { get; set; }
    public string Label { get; init; } = "";
    public override string ToString()
    {
        return Label;
    }
}
