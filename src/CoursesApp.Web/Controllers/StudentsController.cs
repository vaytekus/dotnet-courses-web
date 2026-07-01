using CoursesApp.Web.DTOs;
using CoursesApp.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoursesApp.Web.Controllers
{
    public class StudentsController(
        IStudentService studentService,
        IConfiguration configuration,
        ILogger<StudentsController> logger)
        : Controller
    {
        private const int DefaultPageSize = 10;
        private readonly int _pageSize = configuration.GetValue("Pagination:PageSize", DefaultPageSize);

        public async Task<IActionResult> Index()
        {
            logger.LogInformation("Loading students page");
            var model = await studentService.GetPageAsync(null, null, 1, _pageSize);
            return View(model);
        }

        public async Task<IActionResult> GetStudentsByGroupId(Guid groupId)
        {
            logger.LogInformation("Loading students for group {GroupId}", groupId);
            var students = await studentService.GetStudentsByGroupAsync(groupId);
            return PartialView("_StudentsPartial", students);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] StudentDto? dto)
        {
            if (dto is null)
            {
                return BadRequest("Invalid data");
            }
            try
            {
                await studentService.AddStudentAsync(dto);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding student");
                return Json(new { success = false, message = "Failed to add student" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? search, Guid? groupId, int page = 1)
        {
            logger.LogInformation("Searching students: search={Search}, groupId={GroupId}, page={Page}", search, groupId, page);
            var model = await studentService.GetPageAsync(search, groupId, page, _pageSize);
            return PartialView("_StudentsTableBody", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] StudentEditDto dto)
        {
            try
            {
                await studentService.UpdateStudentAsync(dto);
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
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await studentService.DeleteStudentAsync(id);
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
    }
}