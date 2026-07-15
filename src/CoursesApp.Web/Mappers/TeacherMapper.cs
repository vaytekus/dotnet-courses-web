namespace CoursesApp.Web.Mappers;

public static class TeacherMapper
{
    public static TeacherDto ToDto(this Teacher teacher)
    {
        ArgumentNullException.ThrowIfNull(teacher);
        return new(teacher.Id, teacher.FirstName, teacher.LastName, teacher.DateOfBirth,
            teacher.Email, teacher.Phone, teacher.HireDate, teacher.Degree);
    }

    public static List<TeacherDto> ToDtoList(this List<Teacher> teachers)
    {
        ArgumentNullException.ThrowIfNull(teachers);
        return teachers.Select(t => t.ToDto()).ToList();
    }
}
