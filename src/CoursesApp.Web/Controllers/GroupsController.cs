using CoursesApp.Domain.Enums;
using CoursesApp.Domain.Interfaces;
using CoursesApp.Web.DTOs;
using CoursesApp.Web.Mappers;
using CoursesApp.Web.Models;
using CoursesApp.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoursesApp.Web.Controllers
{
    public class GroupsController(
        IGroupRepository groupRepository, 
        IGroupService groupService,
        ICourseRepository courseRepository,
        ITeacherRepository teacherRepository,
        IConfiguration configuration,
        ILogger<GroupsController> logger)
        : Controller
    {
        private readonly int _pageSize = configuration.GetValue<int>("Pagination:PageSize", 10);
        
        public async Task<IActionResult> Index()
        {
            logger.LogInformation("Loading group with details");
            var (groups, total) = await groupRepository.GetFilteredGroupAsync(null, null, GroupStudentFilter.All, 1, _pageSize);
            var courses = await courseRepository.GetAllCoursesAsync();
            var teachers = await teacherRepository.GetAllTeachersAsync();
            return View(new GroupsIndexViewModel
            {
                Groups = groups.ToGroupDtoList(),
                Courses = courses.ToDtoList(),
                Teachers = teachers.ToDtoList(),
                Page = 1,
                TotalCount = total,
                PageSize = _pageSize
            });
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            string? search,
            Guid? courseId,
            GroupStudentFilter filter = GroupStudentFilter.All,
            int page = 1)
        {
            var (groups, total) = await groupRepository.GetFilteredGroupAsync(search, courseId, filter, page, _pageSize);
            var teachers = await teacherRepository.GetAllTeachersAsync();

            return PartialView("_GroupsBody", new GroupsIndexViewModel
            {
                Groups = groups.ToGroupDtoList(),
                Teachers = teachers.ToDtoList(),
                Page = page,
                TotalCount = total,
                PageSize = _pageSize
            });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] GroupCreateDto? dto)
        {
            if (dto is null)
            {
                return BadRequest("Invalid data");
            }

            await groupService.AddGroupAsync(dto);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] GroupEditDto? dto)
        {
            if (dto is null)
            {
                return BadRequest("Invalid data");
            }
            await groupService.UpdateGroupAsync(dto);
            return Json(new { success = true });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await groupService.DeleteGroupAsync(id);

            if (!deleted)
            {
                return Json(new { success = false, message = "Cannot delete group with students" });
            }
            
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ClearStudents(Guid id)
        {
            await groupService.ClearStudentAsync(id);
            return Json(new { success = true });
        }
    }
}