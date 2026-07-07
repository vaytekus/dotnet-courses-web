namespace CoursesApp.Web.Mappers;

public static class StudentMapper
{
    public static StudentDto ToDto(this Student student) => new(student.Id, student.FirstName, student.LastName, student.GroupId);

    public static List<StudentDto> ToDtoList(this List<Student> students)
    {
        return students.Select(s => s.ToDto()).ToList();
    } 
}
