using CoursesApp.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CoursesApp.Web.Controllers;

public class StudentsController(
    IStudentService studentService,
    IGroupService groupService,
    ICsvService csvService,
    IConfiguration configuration,
    IHubContext<AppHub> hubContext,
    ILogger<StudentsController> logger) : BaseController(logger, configuration)
{
    private const string _csvContentType = "text/csv";
    private const string _csvFileName = "students.csv";

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        logger.LogInformation("Loading students page");
        var model = await BuildViewModelAsync(null, null, 1, ct);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] StudentDto? dto, CancellationToken ct)
    {
        if (dto is null)
        {
            return BadRequest("Invalid data");
        }
        
        return await ExecuteAsync(
            () => studentService.AddStudentAsync(dto, ct), 
            "Error adding student",
            () => hubContext.Clients.All.SendAsync("StudentAdded", dto.GroupId, cancellationToken: ct));
    }

    [HttpGet]
    public async Task<IActionResult> Search(string? search, Guid? groupId, int page = 1, CancellationToken ct = default)
    {
        logger.LogInformation("Searching students: search={Search}, groupId={GroupId}, page={Page}", search, groupId, page);
        var model = await BuildViewModelAsync(search, groupId, page, ct);
        return PartialView("_StudentsTableBody", model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit([FromBody] StudentEditDto dto, CancellationToken ct)
    {
        return await ExecuteAsync(
            () => studentService.UpdateStudentAsync(dto, ct), 
            "Error updating student",
            () => hubContext.Clients.All.SendAsync("StudentUpdated", dto.Id, dto.FirstName, dto.LastName, dto.GroupId, cancellationToken: ct)); 
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(Guid id, Guid groupId, CancellationToken ct)
    {
        return await ExecuteAsync(
            () => studentService.DeleteStudentAsync(id, ct),
            "Error deleting student",
            () => hubContext.Clients.All.SendAsync("StudentDeleted", id, groupId, cancellationToken: ct));
    }

    [HttpPost]
    public async Task<IActionResult> ClearStudents(Guid groupId, CancellationToken ct)
    {
        return await ExecuteAsync(() => studentService.ClearAllStudentsAsync(groupId, ct), "Error clearing students");
    }

    [HttpGet]
    public async Task<IActionResult> GetStudent(Guid groupId, CancellationToken ct)
    {
        logger.LogInformation("Loading students for group {GroupId}", groupId);
        var students = await studentService.GetStudentsByGroupAsync(groupId, ct);
        return PartialView("~/Views/Groups/_StudentsBody.cshtml", students);
    }

    [HttpGet]
    public async Task<IActionResult> ExportStudents(Guid groupId, CancellationToken ct)
    {
        logger.LogInformation("Exporting students for group {GroupId}", groupId);
        var bytes = await csvService.ExportGroupCsvAsync(groupId, ct);
        return File(bytes, _csvContentType, _csvFileName);
    }

    [HttpPost]
    public async Task<IActionResult> ImportStudents(IFormFile? file, Guid groupId, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return Json(new
            {
                success = false,
                message = "No file provided"
            });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await csvService.ImportGroupCsvAsync(stream, groupId, ct);
            return Json(new { success = true, imported = result.ImportedCount, errors = result.Errors });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error importing students for group {GroupId}", groupId);
            return Json(new { success = false, message = "Failed to import students" });
        }
    }

    private async Task<StudentsIndexViewModel> BuildViewModelAsync(string? search, Guid? groupId, int page, CancellationToken ct = default)
    {
        var (students, total) = await studentService.GetPageAsync(search, groupId, page, PageSize, ct);
        var groups = await groupService.GetAllSelectAsync(ct);
        
        return new StudentsIndexViewModel
        {
            Students = students,
            Groups = groups,
            Page = page,
            PageSize = PageSize,
            TotalCount = total
        };
    }
}
