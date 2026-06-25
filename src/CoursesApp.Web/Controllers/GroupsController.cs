using CoursesApp.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CoursesApp.Web.Controllers
{
    public class GroupsController(IGroupRepository groupRepository, ILogger<GroupsController> logger)
        : Controller
    {
        public async Task<IActionResult> Index()
        {
            logger.LogInformation("Loading group with details");
            var group = await groupRepository.GetAllGroupWithDetailsAsync();
            return View(group);
        }

        // public async Task<IActionResult> GetGroup()
        // {
        //     logger.LogInformation("Loading group");
        //     
        // }
    }
}