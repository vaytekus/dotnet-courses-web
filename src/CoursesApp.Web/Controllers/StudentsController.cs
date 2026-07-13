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
        var model = await BuildViewModelAsync(null, null, StudentSortKey.LastName, false, 1, ct);
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
    public async Task<IActionResult> Search(
        string? search, Guid? groupId, int page = 1, 
        StudentSortKey sortKey = StudentSortKey.LastName, bool sortDesc = false, 
        CancellationToken ct = default)
    {
        logger.LogInformation("Searching students: search={Search}, groupId={GroupId}, sort={Sort} desc={Desc}, page={Page}",
            search, groupId, sortKey, sortDesc, page);
        var model = await BuildViewModelAsync(search, groupId, sortKey, sortDesc, page, ct);
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
    public async Task<IActionResult> GetStudent(
        Guid groupId, StudentSortKey sortKey = StudentSortKey.LastName, bool sortDesc = false, 
        int page = 1, CancellationToken ct = default)
    {
        logger.LogInformation("Loading students for group {GroupId}, page {Page}, sort={Sort} desc={Desc}", groupId, page, sortKey, sortDesc);
        var (students, total, effectivePage) = await GetPageAsync(
            null, groupId, sortKey, sortDesc, page, ct);
        
        var model = new GroupStudentsPageViewModel
        {
            Students = students,
            GroupId = groupId,
            Page = effectivePage,
            PageSize = PageSize,
            TotalCount = total,
            SortKey = sortKey,
            SortDesc = sortDesc
        };
        
        return PartialView("~/Views/Groups/_StudentsBody.cshtml", model);
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

    private async Task<StudentsIndexViewModel> BuildViewModelAsync(
        string? search, Guid? groupId, 
        StudentSortKey sortKey, bool sortDesc, int page, 
        CancellationToken ct = default)
    {
        var groups = await groupService.GetAllSelectAsync(ct);
        var (students, total, effectivePage) = await GetPageAsync(search, groupId, sortKey, sortDesc, page, ct);
        
        return new StudentsIndexViewModel
        {
            Students = students,
            Groups = groups,
            Page = effectivePage,
            PageSize = PageSize,
            TotalCount = total,
            SortKey = sortKey,
            SortDesc = sortDesc
        };
    }

    private async Task<(List<StudentDto> students, int total, int page)> GetPageAsync(
        string? search, Guid? groupId, 
        StudentSortKey sortKey, bool sortDesc, int page, 
        CancellationToken ct = default)
    {
        var (students, total) = await studentService.GetPageAsync(search, groupId, sortKey, sortDesc, page, PageSize, ct);
        var totalPages = total > 0 ? (int)Math.Ceiling((double)total / PageSize) : 1;

        if (page > totalPages)
        {
            page = totalPages;
            (students, total) = await studentService.GetPageAsync(search, groupId, sortKey, sortDesc, page, PageSize, ct);
        }
        
        return (students, total, page);
    }
}
