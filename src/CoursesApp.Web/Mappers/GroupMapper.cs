using CoursesApp.Domain.Entities;
using CoursesApp.Web.DTOs;

namespace CoursesApp.Web.Mappers
{
    public static class GroupMapper
    {
        public static GroupDto ToDto(this Group groupDto) => new()
        {
            Id = groupDto.Id,
            Name = groupDto.Name
        };

        public static List<GroupDto> ToDtoList(this List<Group> groups)
        {
            return groups.Select(g => g.ToDto()).ToList();
        }
    }
}