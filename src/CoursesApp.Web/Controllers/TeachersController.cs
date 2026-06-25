using CoursesApp.Domain.Interfaces;
using CoursesApp.Web.DTOs;
using CoursesApp.Web.Mappers;
using CoursesApp.Web.Models;
using CoursesApp.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoursesApp.Web.Controllers
{
    public class TeachersController(
        ITeacherRepository groupRepository, 
        ITeacherService teacherService,
        ILogger<TeachersController> logger)
        : Controller
    {
        public async Task<IActionResult> Index()
        {
            logger.LogInformation("Loading teachers");
            var teachers = await groupRepository.GetAllTeachersAsync();
            return View(new TeachersIndexViewModel
            {
                Teachers = teachers.ToDtoList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] TeacherEditDto dto)
        {
            await teacherService.UpdateTeacherAsync(dto);
            return Json(new { success = true });
        }
    }
}