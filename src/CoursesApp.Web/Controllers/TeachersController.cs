namespace CoursesApp.Web.Controllers;

public class TeachersController(
    ITeacherService teacherService,
    IGroupService groupService,
    IConfiguration configuration,
    ILogger<TeachersController> logger)
    : BaseController(logger, configuration)
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        logger.LogInformation("Loading teachers page");
        
        var model = await BuildViewModelAsync(null, 1, ct);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string? search, int page = 1, CancellationToken ct = default)
    {
        logger.LogInformation("Searching teachers: search={Search}, page={Page}", search, page);
        var model = await BuildViewModelAsync(search, page, ct);
        return PartialView("_TeachersTableBody", model);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] TeacherDto? dto, CancellationToken ct)
    {
        if (dto is null)
        {
            return BadRequest("Invalid data");
        }
        
        return await ExecuteAsync(() => teacherService.AddTeacherAsync(dto, ct), "Error adding teacher");
    }

    [HttpPost]
    public async Task<IActionResult> Edit([FromBody] TeacherEditDto dto, CancellationToken ct)
    {
        return await ExecuteAsync(() => teacherService.UpdateTeacherAsync(dto, ct), "Error updating teacher");
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        return await ExecuteAsync(async () =>                                                                                                                                                                                                           
        {       
            await teacherService.ValidateExistAsync(id, ct);
            await groupService.UnassignTeacherAsync(id, ct);                                                                                                                                                                               
            await teacherService.DeleteTeacherAsync(id, ct);
        }, "Error deleting teacher"); 
    }

    private async Task<TeachersIndexViewModel> BuildViewModelAsync(string? search, int page, CancellationToken ct = default)
    {
        var (teachers, total) = await teacherService.GetPageAsync(search, page, PageSize, ct);
        return new TeachersIndexViewModel
        {
            Teachers = teachers,
            Page = page,
            TotalCount = total,
            PageSize = PageSize
        };
    }
}
