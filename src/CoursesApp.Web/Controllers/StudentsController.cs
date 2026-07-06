using CoursesApp.Web.DTOs;
using CoursesApp.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoursesApp.Web.Controllers
{
    public class StudentsController(
        IStudentService studentService,
        IStudentCsvService studentCsvService,
        IConfiguration configuration,
        ILogger<StudentsController> logger)
        : Controller
    {
        private const int DefaultPageSize = 10;
        private const string _csvContentType = "text/csv";
        private const string _csvFileName = "students.csv";
        private readonly int _pageSize = configuration.GetValue("Pagination:PageSize", DefaultPageSize);

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            logger.LogInformation("Loading students page");
            var model = await studentService.GetPageAsync(null, null, 1, _pageSize, ct);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] StudentDto? dto, CancellationToken ct)
        {
            if (dto is null)
            {
                return BadRequest("Invalid data");
            }
            try
            {
                await studentService.AddStudentAsync(dto, ct);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding student");
                return Json(new { success = false, message = "Failed to add student" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? search, Guid? groupId, int page = 1, CancellationToken ct = default)
        {
            logger.LogInformation("Searching students: search={Search}, groupId={GroupId}, page={Page}", search, groupId, page);
            var model = await studentService.GetPageAsync(search, groupId, page, _pageSize, ct);
            return PartialView("_StudentsTableBody", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] StudentEditDto dto, CancellationToken ct)
        {
            try
            {
                await studentService.UpdateStudentAsync(dto, ct);
                return Json(new { success = true });
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogWarning(ex, "Student not found during edit");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating student");
                return Json(new { success = false, message = "Failed to update student" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                await studentService.DeleteStudentAsync(id, ct);
                return Json(new { success = true });
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogWarning(ex, "Student {Id} not found during delete", id);
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting student {Id}", id);
                return Json(new { success = false, message = "Failed to delete student" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClearStudents(Guid groupId, CancellationToken ct)
        {
            try
            {
                await studentService.ClearAllStudentsAsync(groupId, ct);
                return Json(new { success = true });
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogWarning(ex, "Group {Id} not found during clear students", groupId);
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error clearing students from group {Id}", groupId);
                return Json(new { success = false, message = "Failed to clear students" });
            }
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
            var bytes = await studentCsvService.ExportGroupCsvAsync(groupId, ct);
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
                var result = await studentCsvService.ImportGroupCsvAsync(stream, groupId, ct);
                return Json(new { success = true, imported = result.ImportedCount, errors = result.Errors });
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error importing students for group {GroupId}", groupId);
                return Json(new { success = false, message = "Failed to import students" });
            }
        }
    }
}