using CoursesApp.Domain.Entities;
using CoursesApp.Domain.Enums;

namespace CoursesApp.Infrastructure.Extensions
{
    public static class GroupStudentFilterExtensions
    {
        public static IQueryable<Group> Apply(this GroupStudentFilter filter, IQueryable<Group> query)
            => filter switch
            {
                GroupStudentFilter.WithStudents => query.Where(g => g.Students.Any()),
                GroupStudentFilter.WithoutStudents => query.Where(g => !g.Students.Any()),
                _ => query
            };
    }
}