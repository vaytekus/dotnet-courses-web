using CoursesApp.Domain.Interfaces;
using CoursesApp.Web.DTOs;
using CoursesApp.Web.Mappers;
using CoursesApp.Web.Models;
using CoursesApp.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoursesApp.Web.Controllers
{
    public class StudentsController(
        IStudentRepository studentRepository, 
        IGroupRepository groupRepository,
        IStudentService studentService,
        ILogger<StudentsController> logger)
        : Controller
    {
        public async Task<IActionResult> Index()
        {
            var students = await studentRepository.GetAllStudentsAsync();
            var groups = await groupRepository.GetAllGroupAsync();
            return View(new StudentsIndexViewModel
            {
                Students = students.ToDtoList(),
                Groups = groups.ToDtoList()
            });
        }

        public async Task<IActionResult> GetStudentsByGroupId(Guid groupId)
        {
            logger.LogInformation("Loading students for group {GroupId}", groupId);
            var students = await studentRepository.GetStudentsByGroupAsync(groupId);
            return PartialView("_StudentsPartial", students.ToDtoList());
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] StudentEditDto dto)
        {
            await studentService.UpdateStudentAsync(dto);
            return Json(new { success = true });
        }
        
        // [HttpPost]
        // public async Task<IActionResult> Delete([FromBody] int id)
    }
}