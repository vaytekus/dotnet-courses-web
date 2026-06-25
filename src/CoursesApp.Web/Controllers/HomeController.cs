using System.Diagnostics;
using CoursesApp.Domain.Interfaces;
using CoursesApp.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoursesApp.Web.Controllers;

public class HomeController(ICourseRepository courseRepository, ILogger<HomeController> logger)
    : Controller
{
    public async Task<IActionResult> Index()
    {
        logger.LogInformation("Loading courses");
        var courses = await courseRepository.GetAllCoursesWithDetailsAsync();
        return View(courses);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}