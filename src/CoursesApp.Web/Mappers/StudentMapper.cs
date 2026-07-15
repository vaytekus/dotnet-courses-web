namespace CoursesApp.Web.Mappers;

public static class StudentMapper
{
    public static StudentDto ToDto(this Student student)
    {
        ArgumentNullException.ThrowIfNull(student);
        return new(student.Id, student.FirstName, student.LastName, student.GroupId,
            student.DateOfBirth, student.Gender, student.TaxId,
            student.Email, student.Phone, student.City, student.Street, student.PostalCode);
    }

    public static List<StudentDto> ToDtoList(this List<Student> students)
    {
        ArgumentNullException.ThrowIfNull(students);
        return students.Select(s => s.ToDto()).ToList();
    }
}
