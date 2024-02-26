using CoffeeLands.Data;
using CoffeeLands.Models;
using CoffeeLands.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CoffeeLands.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly CoffeeLandsContext _context;
        public CartViewComponent(CoffeeLandsContext context) => _context = context;

        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.GetString("Cart");
            if (cart != null)
            {
                var userList = JsonConvert.DeserializeObject<List<CartItem>>(cart);
                if (userList != null)
                {
                    ViewBag.Quantity = userList.Count;
                }
            }
            else
            {
                ViewBag.Quantity = 0;
            }
            return View("Cart");
        }
    }
}
