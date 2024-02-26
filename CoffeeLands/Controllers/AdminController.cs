using CoffeeLands.Data;
using CoffeeLands.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Claims;

namespace CoffeeLands.Controllers
{
    public class AdminController : Controller
    {

        private readonly CoffeeLandsContext _context;

        public AdminController(CoffeeLandsContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var checkRole = User.FindFirst(ClaimTypes.Role);
            if (checkRole.Value == "ADMIN")
            {
                return View("~/Views/Admin/Pages/Index.cshtml");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
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
            ViewData["OrderSortParm"] = String.IsNullOrEmpty(sortOrder) ? "order_desc" : "";
            var orderDetails = from op in _context.OrderDetail
                .Include(o => o.OrderProduct)
                .Include(p => p.Product)
                               select op;
            switch (sortOrder)
            {
                case "order_desc":
                    orderDetails = orderDetails.OrderByDescending(s => s.OrderProductID);
                    break;
                default:
                    orderDetails = orderDetails.OrderBy(s => s.OrderProductID);
                    break;
            }
            int pageSize = 10;
            return View("~/Views/Admin/Pages/OrderDetail.cshtml", await PaginatedList<OrderDetail>.CreateAsync(orderDetails.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
    }
}
