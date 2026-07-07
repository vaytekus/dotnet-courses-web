namespace CoursesApp.Web.Mappers;

public static class TeacherMapper
{
    public static TeacherDto ToDto(this Teacher groupDto) => new(groupDto.Id, groupDto.FirstName, groupDto.LastName);

    public static List<TeacherDto> ToDtoList(this List<Teacher> teachers)
    {
        return teachers.Select(t => t.ToDto()).ToList();
    }
}
