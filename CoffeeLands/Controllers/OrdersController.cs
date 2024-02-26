using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CoffeeLands.Data;
using CoffeeLands.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using PayPal.Api;
using Microsoft.Extensions.Configuration;
using CoffeeLands.Services;
using Azure;
using CoffeeLands.ViewModels.Paypal;
using CoffeeLands.ViewModels.Mail;
using CoffeeLands.ViewModels.VNPay;
using CoffeeLands.ViewModels.Momo;
using CoffeeLands.Helpers;
using CoffeeLands.ViewModels;
using Microsoft.AspNetCore.Authorization;



namespace CoffeeLands.Controllers
{
    public class OrdersController : Controller
    {
        private readonly CoffeeLandsContext _context;
        public OrdersController(CoffeeLandsContext context)
        {
            _context = context;
        }
        // Index Orders
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["IDSortParm"] = String.IsNullOrEmpty(sortOrder) ? "id_desc" : "";
            ViewData["NameSortParm"] = sortOrder == "name_asc" ? "name_asc" : "name_desc";
            
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var orders = from o in _context.OrderProduct
                           .Include(u => u.User)
                         select o;
            if (!String.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(s => s.Name.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "id_desc":
                    orders = orders.OrderByDescending(s => s.Id);
                    break;
                case "name_asc":
                    orders = orders.OrderBy(s => s.Name);
                    break;
                case "name_desc":
                    orders = orders.OrderByDescending(s => s.Name);
                    break;
                default:
                    orders = orders.OrderBy(s => s.Id);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<OrderProduct>.CreateAsync(orders.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.OrderProduct
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        private bool OrderExists(int id)
        {
            return _context.OrderProduct.Any(e => e.Id == id);
        }
    }
}
