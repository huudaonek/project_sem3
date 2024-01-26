using CoffeeLands.Data;
using CoffeeLands.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;

namespace CoffeeLands.Controllers
{
    public class AdminController : Controller
    {

        private readonly CoffeeLandsContext _context;

        public AdminController(CoffeeLandsContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Admin") != null)
            {
                ViewBag.UserAdmin = HttpContext.Session.GetString("Admin").ToString();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

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

        public async Task<IActionResult> OrderDetails(
    string sortOrder,

    int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["PriceSortParm"] = String.IsNullOrEmpty(sortOrder) ? "price_desc" : "";


            var orderProducts = from op in _context.OrderProduct
                .Include(o => o.Order)
                .Include(p => p.Product)
                select op;

            switch (sortOrder)
            {
                case "price_desc":
                    orderProducts = orderProducts.OrderByDescending(s => s.Price);
                    break;
                default:
                    orderProducts = orderProducts.OrderBy(s => s.Price);
                    break;
            }

            int pageSize = 2;
            
            return View("~/Views/Admin/Pages/OrderDetail.cshtml", await PaginatedList<OrderProduct>.CreateAsync(orderProducts.AsNoTracking(), pageNumber ?? 1, pageSize));
        }




    }
}
