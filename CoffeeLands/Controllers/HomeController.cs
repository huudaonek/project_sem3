using CoffeeLands.Data;
using CoffeeLands.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


namespace CoffeeLands.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;
        

        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}

        public IActionResult Index()
        {
            return View("~/Views/Home/Pages/Index.cshtml");
        }

        private readonly CoffeeLandsContext _context;

        public HomeController(CoffeeLandsContext context)
        {
            _context = context;
        }

        // GET: Coffees
        public async Task<IActionResult> Menu()
        {
            var products = await _context.Product.ToListAsync();
            return View(products);
        }

        public IActionResult Services()
        {
            return View("~/Views/Home/Pages/Services.cshtml");
        }

        public IActionResult Blog()
        {
            return View("~/Views/Home/Pages/Blog.cshtml");
        }

        public IActionResult About()
        {
            return View("~/Views/Home/Pages/About.cshtml");
        }

        public IActionResult Privacy()
        {
            return View("~/Views/Home/Pages/Privacy.cshtml");
        }
        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
