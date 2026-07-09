namespace CoursesApp.Web.Controllers;

public abstract class BaseController(
    ILogger logger, 
    IConfiguration configuration) : Controller
{
    private const int DefaultPageSize = 10;
    protected readonly int PageSize = configuration.GetValue("Pagination:PageSize", DefaultPageSize);
    
    protected async Task<IActionResult> ExecuteAsync(Func<Task> action, string errorMessage, Func<Task>? afterSuccess = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(errorMessage);
        try
        {
            await action();
            if (afterSuccess is not null) await afterSuccess();
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Fail(ex, errorMessage);
        }
    }

    protected async Task<IActionResult> ExecuteAsync(Func<Task<bool>> action, string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(errorMessage);
        try
        {
            var result = await action();
            return Json(new { success = result });
        }
        catch (Exception ex)
        {
            return Fail(ex, errorMessage);
        }
    }

    protected async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> action, string errorMessage, Func<T, Task>? afterSuccess = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(errorMessage);
        try
        {
            var result = await action();
            if (afterSuccess is not null) await afterSuccess(result);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Fail(ex, errorMessage);
        }
    }

    private IActionResult Fail(Exception ex, string errorMessage)
    {
        if (ex is KeyNotFoundException)
        {
            logger.LogWarning(ex, errorMessage);
        }
        else
        {
            logger.LogError(ex, errorMessage);
        }
        
        return Json(new { success = false, message = errorMessage });
    }
}
