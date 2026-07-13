namespace CoursesApp.Web.Extensions;

public static class PaginationHelper
{
    private const int _minPageCount = 1;

    public static async Task<(TData Data, int Total, int Page)> ClampPageAsync<TData>(
        int page,
        int pageSize,
        Func<int, Task<(TData Data, int Total)>> fetch)
    {
        ArgumentNullException.ThrowIfNull(fetch);

        var (data, total) = await fetch(page);
        var totalPages = total > 0 ? (int)Math.Ceiling((double)total / pageSize) : _minPageCount;
        
        if (page > totalPages)
        {
            page = totalPages;
            (data, total) = await fetch(page);
        }
        
        return (data, total, page);
    }
}
