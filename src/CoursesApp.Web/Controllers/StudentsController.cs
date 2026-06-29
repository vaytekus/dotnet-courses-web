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
        IConfiguration configuration,
        ILogger<StudentsController> logger)
        : Controller
    {
        private readonly int _pageSize = configuration.GetValue<int>("Pagination:PageSize", 10);
        public async Task<IActionResult> Index()
        {
            var (students, total) = await studentRepository.GetFilteredStudentAsync(null, null, 1, _pageSize);
            var groups = await groupRepository.GetAllGroupAsync();
            
            return View(new StudentsIndexViewModel
            {
                Students = students.ToDtoList(),
                Groups = groups.ToSelectDtoList(),
                Page = 1,
                TotalCount = total,
                PageSize = _pageSize
            });
        }

        public async Task<IActionResult> GetStudentsByGroupId(Guid groupId)
        {
            logger.LogInformation("Loading students for group {GroupId}", groupId);
            var students = await studentRepository.GetStudentsByGroupAsync(groupId);
            return PartialView("_StudentsPartial", students.ToDtoList());
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] StudentDto? dto)
        {
            if (dto is null)
            {
                return BadRequest("Invalid data");
            }
            
            await studentService.AddStudentAsync(dto);
            return Json(new { success = true });
        }
        
        [HttpGet]
        public async Task<IActionResult> Search(string? search, Guid? groupId, int page = 1)
        {
            var (students, total) = await studentRepository.GetFilteredStudentAsync(search, groupId, page, _pageSize);
            var groups = await groupRepository.GetAllGroupAsync();
            
            return PartialView("_StudentsTableBody", new StudentsIndexViewModel
            {
                Students = students.ToDtoList(),
                Groups = groups.ToSelectDtoList(),
                Page = page,
                TotalCount = total,
                PageSize = _pageSize
            });
        }
        
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] StudentEditDto dto)
        {
            await studentService.UpdateStudentAsync(dto);
            return Json(new { success = true });
        }
        
        [HttpDelete]
        public async Task<IActionResult> Delete(Guid id)
        {
            await studentService.DeleteStudentAsync(id);
            return Json(new { success = true });
        }
    }
}