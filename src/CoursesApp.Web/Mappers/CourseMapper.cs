namespace CoursesApp.Web.Mappers;

public static class CourseMapper
{
    public static CourseDto ToDto(this Course course)
    {
        ArgumentNullException.ThrowIfNull(course);
        return new(course.Id, course.Name);
    }

    public static List<CourseDto> ToDtoList(this List<Course> courses)
    {
        ArgumentNullException.ThrowIfNull(courses);
        return courses.Select(c => c.ToDto()).ToList();
    }
}
