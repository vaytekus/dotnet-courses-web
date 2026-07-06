using CoursesApp.Domain.Enums;
using CoursesApp.Web.DTOs;
using CoursesApp.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoursesApp.Web.Controllers
{
    public class GroupsController(
        IGroupService groupService,
        IConfiguration configuration,
        ILogger<GroupsController> logger)
        : Controller
    {
        private const int DefaultPageSize = 10;
        private readonly int _pageSize = configuration.GetValue("Pagination:PageSize", DefaultPageSize);

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            logger.LogInformation("Loading groups page");
            var model = await groupService.GetPageAsync(null, null, GroupStudentFilter.All, 1, _pageSize, ct);
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
            var model = await groupService.GetPageAsync(search, courseId, filter, page, _pageSize, ct);
            return PartialView("_GroupsBody", model);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] GroupCreateDto? dto,CancellationToken ct)
        {
            if (dto is null)
            {
                return BadRequest("Invalid data");
            }
            try
            {
                await groupService.AddGroupAsync(dto, ct);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error adding group");
                return Json(new { success = false, message = "Failed to add group" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] GroupEditDto? dto, CancellationToken ct)
        {
            if (dto is null)
            {
                return BadRequest("Invalid data");
            }
            try
            {
                await groupService.UpdateGroupAsync(dto, ct);
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
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            try
            {
                var deleted = await groupService.DeleteGroupAsync(id, ct);
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
    }
}