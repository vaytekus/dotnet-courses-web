using CoursesApp.Domain.Interfaces;
using CoursesApp.Web.DTOs;
using CoursesApp.Web.Mappers;
using CoursesApp.Web.Models;
using CoursesApp.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoursesApp.Web.Controllers
{
    public class TeachersController(
        ITeacherRepository teacherRepository, 
        ITeacherService teacherService,
        IConfiguration configuration,
        ILogger<TeachersController> logger)
        : Controller
    {
        private readonly int _pageSize = configuration.GetValue<int>("Pagination:PageSize", 10);
        public async Task<IActionResult> Index()
        {
            logger.LogInformation("Loading teachers");
            var (teachers, total) = await teacherRepository.GetFilteredTeachersAsync(null, 1, _pageSize);
            
            return View(new TeachersIndexViewModel
            {
                Teachers = teachers.ToDtoList(),
                Page = 1,
                TotalCount = total,
                PageSize = _pageSize
            });
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] TeacherDto? dto)
        {
            if (dto is null)
            {
                return BadRequest("Invalid data");
            }

            await teacherService.AddTeacherAsync(dto);
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? search, int page = 1)
        {
            var (teachers, total) = await teacherRepository.GetFilteredTeachersAsync(search, page, _pageSize);

            return PartialView("_TeachersTableBody", new TeachersIndexViewModel
            {
                Teachers = teachers.ToDtoList(),
                Page = page,
                TotalCount = total,
                PageSize = _pageSize
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] TeacherEditDto dto)
        {
            await teacherService.UpdateTeacherAsync(dto);
            return Json(new { success = true });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            await teacherService.DeleteTeacherAsync(id);
            return Json(new { success = true });
        }
    }
}