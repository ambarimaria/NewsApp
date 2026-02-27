using Microsoft.AspNetCore.Mvc;

namespace NewsApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => RedirectToAction("Index", "News");
    }
}
