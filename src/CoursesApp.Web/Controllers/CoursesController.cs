using System.Diagnostics;
using CoursesApp.Domain.Interfaces;
using CoursesApp.Web.Models;
using CoursesApp.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoursesApp.Web.Controllers;

public class CoursesController(
    ICourseService courseService, 
    ILogger<CoursesController> logger) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        logger.LogInformation("Loading courses");
        var courses = await courseService.GetAllWithDetailsAsync(ct);
        return View(courses);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}