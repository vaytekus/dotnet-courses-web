namespace CoursesApp.Web.Mappers;

public static class GroupMapper
{
    public static GroupSelectDto ToSelectDto(this Group groupDto) => new(groupDto.Id, groupDto.Name);

    public static List<GroupSelectDto> ToSelectDtoList(this List<Group> groups)
    {
        return groups.Select(g => g.ToSelectDto()).ToList();
    }

    public static GroupDto ToGroupDto(this Group group) => new()
    {
        Id = group.Id,
        Name = group.Name,
        Course = group.Course,
        CourseId = group.CourseId,
        Teacher = group.Teacher,
        TeacherId = group.TeacherId,
        Students = group.Students
    };
    
    public static List<GroupDto> ToGroupDtoList(this List<Group> groups)
        => groups.Select(g => g.ToGroupDto()).ToList();
}
