using CoursesApp.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CoursesApp.Web.Controllers;

public class GroupsController(
    IGroupService groupService,
    IConfiguration configuration,
    IHubContext<AppHub> hubContext,
    ILogger<GroupsController> logger)
    : BaseController(logger, configuration)
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        logger.LogInformation("Loading groups page");
        var model = await groupService.GetPageAsync(null, null, GroupStudentFilter.All, 1, PageSize, ct);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        string? search,
        Guid? courseId,
        GroupStudentFilter filter = GroupStudentFilter.All,
        int page = 1,
        CancellationToken ct = default)
    {
        logger.LogInformation("Searching groups: search={Search}, courseId={CourseId}, filter={Filter}, page={Page}", search, courseId, filter, page);
        var model = await groupService.GetPageAsync(search, courseId, filter, page, PageSize, ct);
        return PartialView("_GroupsBody", model);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] GroupCreateDto? dto, CancellationToken ct)
    {
        if (dto is null)
        {
            return BadRequest("Invalid data");
        }
        
        return await ExecuteAsync(
            () => groupService.AddGroupAsync(dto, ct),
            "Error adding group",
            id => hubContext.Clients.All.SendAsync("GroupAdded", id, dto.Name.Trim(), cancellationToken: ct));
    }

    [HttpPost]
    public async Task<IActionResult> Edit([FromBody] GroupEditDto? dto, CancellationToken ct)
    {
        if (dto is null)
        {
            return BadRequest("Invalid data");
        }
        
        return await ExecuteAsync(
        () => groupService.UpdateGroupAsync(dto, ct),
        "Error updating group",
        () => hubContext.Clients.All.SendAsync("GroupUpdated", dto.Id, dto.Name.Trim(), cancellationToken: ct));
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(Guid id, bool deleteStudents = false, CancellationToken ct = default)
    {
        try
        {
            var result = await groupService.DeleteGroupAsync(id, deleteStudents, ct);
            if (result.Success)
            {
                await hubContext.Clients.All.SendAsync("GroupDeleted", id, result.DeletedStudentsIds, cancellationToken: ct);
            }
            return Json(new { success = result.Success });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting group");
            return Json(new { success = false, message = "Error deleting group" }); 
        }
    }
}
