using CoffeeLands.Data;
using CoffeeLands.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using PayPal.Api;

namespace CoffeeLands.Controllers
{
    public class HomeController : Controller
    {
        private readonly CoffeeLandsContext _coffeeContext;
        private readonly ILogger<HomeController> _logger;
		private IHttpContextAccessor _contextAccessor;
		IConfiguration _configuration;
		public HomeController(ILogger<HomeController> logger, CoffeeLandsContext coffeeContext, IHttpContextAccessor context, IConfiguration iconfiguration)
        {
            _logger = logger;
            _coffeeContext = coffeeContext;
			_contextAccessor = context;
			_configuration = iconfiguration;
		}

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserSession") != null)
            {
                ViewBag.MySession = HttpContext.Session.GetString("UserSession").ToString();
            }
                ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");
            return View("~/Views/Home/Pages/Index.cshtml");
        }

        public async Task<IActionResult> Menu()
        {
            if (HttpContext.Session.GetString("UserSession") != null)
            {
                ViewBag.MySession = HttpContext.Session.GetString("UserSession").ToString();
            }
            var products = await _coffeeContext.Product.Take(4).ToListAsync();
            ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");
            return View("~/Views/Home/Pages/Menu.cshtml", products);
        }

        public IActionResult Services()
        {

            if (HttpContext.Session.GetString("UserSession") != null)
            {
                ViewBag.MySession = HttpContext.Session.GetString("UserSession").ToString();
            }
            ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");
            return View("~/Views/Home/Pages/Services.cshtml");
        }

        public IActionResult Blog()
        {

            if (HttpContext.Session.GetString("UserSession") != null)
            {
                ViewBag.MySession = HttpContext.Session.GetString("UserSession").ToString();
            }
            ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");
            return View("~/Views/Home/Pages/Blog.cshtml");
        }

        public IActionResult About()
        {

            if (HttpContext.Session.GetString("UserSession") != null)
            {
                ViewBag.MySession = HttpContext.Session.GetString("UserSession").ToString();
            }
            ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");
            return View("~/Views/Home/Pages/About.cshtml");
        }

        public IActionResult Contact()
        {
            if (HttpContext.Session.GetString("UserSession") != null)
            {
                ViewBag.MySession = HttpContext.Session.GetString("UserSession").ToString();
            }
            ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");
            return View("~/Views/Home/Pages/Contact.cshtml");
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
