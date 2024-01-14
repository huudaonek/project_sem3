using Microsoft.AspNetCore.Mvc;

namespace CoffeeLands.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/Admin/Pages/Index.cshtml");
        }

        public IActionResult Index2()
        {
            return View("~/Views/Admin/Pages/Index2.cshtml");
        }

        public IActionResult Support()
        {
            return View();
        }
    }
}
