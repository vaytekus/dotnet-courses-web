using CoursesApp.Web.DTOs;
using CoursesApp.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoursesApp.Web.Controllers
{
    public class TeachersController(
        ITeacherService teacherService,
        IGroupService groupService,
        IConfiguration configuration,
        ILogger<TeachersController> logger)
        : Controller
    {
        private const int DefaultPageSize = 10;
        private readonly int _pageSize = configuration.GetValue("Pagination:PageSize", DefaultPageSize);

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            logger.LogInformation("Loading teachers page");
            var model = await teacherService.GetPageAsync(null, 1, _pageSize, ct);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? search, int page = 1, CancellationToken ct = default)
        {
            logger.LogInformation("Searching teachers: search={Search}, page={Page}", search, page);
            var model = await teacherService.GetPageAsync(search, page, _pageSize, ct);
            return PartialView("_TeachersTableBody", model);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] TeacherDto? dto, CancellationToken ct)
        {
            if (dto is null)
            {
                return BadRequest("Invalid data");
            }
            try
            {
                await teacherService.AddTeacherAsync(dto, ct);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding teacher");
                return Json(new { success = false, message = "Failed to add teacher" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] TeacherEditDto dto, CancellationToken ct)
        {
            try
            {
                await teacherService.UpdateTeacherAsync(dto, ct);
                return Json(new { success = true });
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogWarning(ex, "Teacher not found during edit");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating teacher");
                return Json(new { success = false, message = "Failed to update teacher" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                await teacherService.ValidateExistAsync(id, ct);
                await groupService.UnassignTeacherAsync(id, ct);
                await teacherService.DeleteTeacherAsync(id, ct);
                return Json(new { success = true });
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogWarning(ex, "Teacher {Id} not found during delete", id);
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting teacher {Id}", id);
                return Json(new { success = false, message = "Failed to delete teacher" });
            }
        }
    }
}