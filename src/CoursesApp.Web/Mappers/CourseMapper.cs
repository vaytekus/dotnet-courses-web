namespace CoursesApp.Web.Mappers;

public static class CourseMapper
{
    public static CourseDto ToDto(this Course course) => new(course.Id, course.Name);
    
    public static List<CourseDto> ToDtoList(this List<Course> courses)
        => courses.Select(c => c.ToDto()).ToList();
}
