using CoursesApp.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CoursesApp.Web.Controllers;

public class TeachersController(
    ITeacherService teacherService,
    IGroupService groupService,
    IConfiguration configuration,
    IHubContext<AppHub> hubContext,
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
        
        return await ExecuteAsync(
            () => teacherService.AddTeacherAsync(dto, ct),
            "Error adding teacher",
            id => hubContext.Clients.All.SendAsync("TeacherAdded", id, dto.FirstName, dto.LastName, cancellationToken: ct));
    }

    [HttpPost]
    public async Task<IActionResult> Edit([FromBody] TeacherEditDto dto, CancellationToken ct)
    {
        return await ExecuteAsync(
            () => teacherService.UpdateTeacherAsync(dto, ct),
            "Error updating teacher",
            () => hubContext.Clients.All.SendAsync("TeacherUpdated", dto.Id, dto.FirstName, dto.LastName, cancellationToken: ct));
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        return await ExecuteAsync(async () =>                                                                                                                                                                                                           
        {       
            await teacherService.ValidateExistAsync(id, ct);
            await groupService.UnassignTeacherAsync(id, ct);                                                                                                                                                                               
            await teacherService.DeleteTeacherAsync(id, ct);
        }, 
        "Error deleting teacher",
        () => hubContext.Clients.All.SendAsync("TeacherDeleted", id, cancellationToken: ct));
    }

    private async Task<TeachersIndexViewModel> BuildViewModelAsync(string? search, int page, CancellationToken ct = default)
    {
        var (teachers, total, effectivePage) = await GetPageAsync(search, page, ct);
        return new TeachersIndexViewModel
        {
            Teachers = teachers,
            Page = effectivePage,
            TotalCount = total,
            PageSize = PageSize
        };
    }
    
    private async Task<(List<TeacherDto> teachers, int total, int page)> GetPageAsync(string? search, int page, CancellationToken ct = default)
    {
        var (teachers, total) = await teacherService.GetPageAsync(search, page, PageSize, ct);
        var totalPages = total > 0 ? (int)Math.Ceiling((double)total / PageSize) : 1;
        if (page > totalPages)
        {
            page = totalPages;
            (teachers, total) = await teacherService.GetPageAsync(search, page, PageSize, ct);
        }

        return (teachers, total, page);
    }
}
