namespace CoursesApp.Infrastructure.Extensions;
    
public static class TeacherQueryExtensions
{
    public static IQueryable<Teacher> ApplySort(this IQueryable<Teacher> query, TeacherSortKey sortKey, bool sortDesc)
    {
        return (sortKey, sortDesc) switch
        {
            (TeacherSortKey.FirstName, false) => query.OrderBy(s => s.FirstName),
            (TeacherSortKey.FirstName, true) => query.OrderByDescending(s => s.FirstName),
            (TeacherSortKey.LastName, true) => query.OrderByDescending(s => s.LastName),
            _ => query.OrderBy(s => s.LastName)
        };
    }
}
