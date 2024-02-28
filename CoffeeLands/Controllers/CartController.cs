using CoffeeLands.Data;
using CoffeeLands.Helpers;
using CoffeeLands.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeLands.Controllers
{
    public class CartController : Controller
    {
        private readonly CoffeeLandsContext _context;
        
        public CartController(CoffeeLandsContext context)
        {
            _context = context;
        }
        public List<CartItem> MyCart => HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();

        [Authorize]
        public IActionResult Index()
        {

            var cart = MyCart;
            if (cart != null)
            {
                decimal total = 0;
                ViewBag.Cart = MyCart;
                foreach (CartItem cartItem in cart)
                {
                    total += cartItem.SubTotal;
                }
                ViewBag.Total = total;
                ViewBag.GrandTotal = Math.Round(total * 1.1m, 2);
                return View(cart);
            }
            return View(new List<CartItem>());
        }

        [Authorize]
        public async Task<IActionResult> AddToCart(int? id, int buy_qty)
        {
            var userName = User.Identity.Name;
            var cart = MyCart;
            var item = cart.SingleOrDefault(p => p.ProductID == id);

            if (item == null)
            {
                var product = await _context.Product.SingleOrDefaultAsync(p => p.Id == id);
                var user = await _context.User.SingleOrDefaultAsync(u => u.Name == userName);
                if (product == null)
                {
                    TempData["Error"] = $"Không tìm thấy hàng hóa có mã {id}";
                    return Redirect("/ProductDetail");
                }
                item = new CartItem
                {
                    ProductID = product.Id,
                    UserID = user.Id,
                    Name = product.Name,
                    Image = product.Image,
                    Price = product.Price,
                    Description = product.Description,
                    Qty = buy_qty
                };
                cart.Add(item);
            }
            else
            {
                item.Qty += buy_qty;
            }
            HttpContext.Session.Set("Cart", cart);
            //TempData["Notification"] = "Add to cart success!";
            return RedirectToAction("Index", "Cart");
        }

        public IActionResult RemoveToCart(int? productId, int? userId)
        {
            if (productId == null)
            {
                return NotFound();
            }
            var cart = MyCart;
            if (cart != null)
            {
                var productToRemove = cart.FirstOrDefault(p => p.ProductID == productId && p.UserID == userId);
                if (productToRemove != null)
                {
                    cart.Remove(productToRemove);
                    HttpContext.Session.Set("Cart", cart);
                }
            }
            return RedirectToAction("Index", "Cart");
        }
    }
}
