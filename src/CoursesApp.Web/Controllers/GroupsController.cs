using CoursesApp.Domain.Enums;
using CoursesApp.Web.DTOs;
using CoursesApp.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoursesApp.Web.Controllers
{
    public class GroupsController(
        IGroupService groupService,
        IStudentService studentService,
        IConfiguration configuration,
        ILogger<GroupsController> logger)
        : Controller
    {
        private const int DefaultPageSize = 10;
        private readonly int _pageSize = configuration.GetValue("Pagination:PageSize", DefaultPageSize);

        public async Task<IActionResult> Index()
        {
            logger.LogInformation("Loading groups page");
            var model = await groupService.GetPageAsync(null, null, GroupStudentFilter.All, 1, _pageSize);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            string? search,
            Guid? courseId,
            GroupStudentFilter filter = GroupStudentFilter.All,
            int page = 1)
        {
            logger.LogInformation("Searching groups: search={Search}, courseId={CourseId}, filter={Filter}, page={Page}", search, courseId, filter, page);
            var model = await groupService.GetPageAsync(search, courseId, filter, page, _pageSize);
            return PartialView("_GroupsBody", model);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] GroupCreateDto? dto)
        {
            if (dto is null)
            {
                return BadRequest("Invalid data");
            }
            try
            {
                await groupService.AddGroupAsync(dto);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding group");
                return Json(new { success = false, message = "Failed to add group" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] GroupEditDto? dto)
        {
            if (dto is null)
            {
                return BadRequest("Invalid data");
            }
            try
            {
                await groupService.UpdateGroupAsync(dto);
                return Json(new { success = true });
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogWarning(ex, "Group not found during edit");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating group");
                return Json(new { success = false, message = "Failed to update group" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await groupService.DeleteGroupAsync(id);
                return Json(new { success = deleted });
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogWarning(ex, "Group {Id} not found during delete", id);
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting group {Id}", id);
                return Json(new { success = false, message = "Failed to delete group" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClearStudents(Guid id)
        {
            try
            {
                await studentService.ClearAllStudentsAsync(id);
                return Json(new { success = true });
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogWarning(ex, "Group {Id} not found during clear students", id);
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error clearing students from group {Id}", id);
                return Json(new { success = false, message = "Failed to clear students" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent(Guid id)
        {
            try
            {
                await studentService.DeleteStudentAsync(id);
                return Json(new { success = true });
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogWarning(ex, "Student {Id} not found during delete from group", id);
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting student {Id} from group", id);
                return Json(new { success = false, message = "Failed to delete student" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStudent(Guid groupId)
        {
            logger.LogInformation("Loading students for group {GroupId}", groupId);
            var students = await studentService.GetStudentsByGroupAsync(groupId);
            return PartialView("_StudentsBody", students);
        }

        [HttpGet]
        public async Task<IActionResult> ExportStudents(Guid groupId)
        {
            logger.LogInformation("Exporting students for group {GroupId}", groupId);
            var bytes = await studentService.ExportGroupCsvAsync(groupId);
            return File(bytes, "text/csv", "students.csv");
        }

        [HttpPost]
        public async Task<IActionResult> ImportStudents(IFormFile? file, Guid groupId)
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
                var result = await studentService.ImportGroupCsvAsync(stream, groupId);
                return Json(new {success = true, imported = result.ImportedCount, errors = result.Errors });
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error importing students for group {GroupId}", groupId);
                return Json(new { success = false, message = "Failed to import students" });
            }
        }
    }
}