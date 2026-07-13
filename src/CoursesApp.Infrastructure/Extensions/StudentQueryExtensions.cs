namespace CoursesApp.Infrastructure.Extensions;
    
public static class StudentQueryExtensions
{
    public static IQueryable<Student> ApplySort(this IQueryable<Student> query, StudentSortKey sortKey, bool sortDesc)
    {
        return (sortKey, sortDesc) switch
        {
            (StudentSortKey.FirstName, false) => query.OrderBy(s => s.FirstName),
            (StudentSortKey.FirstName, true) => query.OrderByDescending(s => s.FirstName),
            (StudentSortKey.LastName, true) => query.OrderByDescending(s => s.LastName),
            _ => query.OrderBy(s => s.LastName)
        };
    }
}
