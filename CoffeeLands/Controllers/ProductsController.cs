using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CoffeeLands.Data;
using CoffeeLands.Models;
using Microsoft.AspNetCore.Http;
//using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;

namespace CoffeeLands.Controllers
{

    public class ProductsController : Controller
    {
        private readonly CoffeeLandsContext _context;

        private readonly IWebHostEnvironment _hostingEnvironment;
        public ProductsController(CoffeeLandsContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: Products
        public async Task<IActionResult> Index(
    string sortOrder,
    string currentFilter,
    string searchString,
    int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var products = from s in _context.Product
                           .Include(c => c.Category)
                           select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                products = products.Where(s => s.Name.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(s => s.Name);
                    break;
                default:
                    products = products.OrderBy(s => s.Name);
                    break;
            }

            int pageSize = 3;
            return View(await PaginatedList<Product>.CreateAsync(products.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        //Product Detail Customer
        public async Task<IActionResult> ProductDetail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (HttpContext.Session.GetString("UserSession") != null)
            {
                ViewBag.MySession = HttpContext.Session.GetString("UserSession").ToString();
            }
            ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");

            var product = await _context.Product
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        //Add To Cart

        public async Task<IActionResult> AddToCart(int? id, int buy_qty)
        {
            var userID = HttpContext.Session.GetInt32("UserId");
            var checkUser = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(checkUser))
            {
                return RedirectToAction("Login", "Users");
            }
            if (id == null)
            {
                //ViewBag.Cart = "Add To Cart Failed!";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var product = await _context.Product
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
                var user = await _context.User
                .FirstOrDefaultAsync(u => u.Id == userID);

                if (product != null && user != null)
                {
                    //string productList = JsonConvert.SerializeObject(products);
                    //HttpContext.Session.SetString("Cart", productList);

                    //string userCart = JsonConvert.SerializeObject(user);
                    //HttpContext.Session.SetString("userCart", userCart);

                    //HttpContext.Session.SetInt32("buyQty", buy_qty);

                    ProductCart pdc = new ProductCart();
                    pdc.Qty = buy_qty;
                    pdc.CartProduct = new Product
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Image = product.Image,
                        Price = product.Price,
                        Description = product.Description
                    };
                    pdc.CartUser = new User
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Password = user.Password,
                        Role = user.Role,
                    };
                    //await _context.SaveChangesAsync();

                    // Lưu đối tượng vào Session
                    var productListJson = HttpContext.Session.GetString("Cart");
                    var productList = new List<ProductCart>();
                    if (!string.IsNullOrEmpty(productListJson))
                    {
                        // Nếu Session đã chứa danh sách, thì deserialize nó và thêm mới
                        productList = JsonConvert.DeserializeObject<List<ProductCart>>(productListJson);
                    }
                    ProductCart existingProductCart = productList.FirstOrDefault(p => p.CartProduct.Id == product.Id && p.CartUser.Id == user.Id);
                    if (existingProductCart != null)
                    {
                        existingProductCart.Qty += buy_qty;
                    }
                    else
                    {
                        productList.Add(pdc);
                    }
                    string updatedProductListJson = JsonConvert.SerializeObject(productList);
                    HttpContext.Session.SetString("Cart", updatedProductListJson);
                    HttpContext.Session.SetString("CartNumber", productList.Count.ToString());
                    return RedirectToAction("Cart", "Products");
                }
            }
            return View("~/Views/Test/ProductDetail.cshtml");
        }


        //Cart
        public async Task<IActionResult> Cart()
        {
            var checkUser = HttpContext.Session.GetString("UserSession");
            if (string.IsNullOrEmpty(checkUser))
            {
                return RedirectToAction("Login", "Users");
            }
            else
            {
                ViewBag.MySession = checkUser.ToString();
            }
            ViewBag.CartNumber = HttpContext.Session.GetString("CartNumber");

            //List<Product> products = new List<Product>();
            //string productString = HttpContext.Session.GetString("Cart");
            //products = JsonConvert.DeserializeObject<List<Product>>(productString);

            //User user = new User();
            //string userString = HttpContext.Session.GetString("userCart");
            //user = JsonConvert.DeserializeObject<User>(productString);

            //var buyQty = HttpContext.Session.GetInt32("buyQty");
            //if (buyQty != null)
            //{

            //    ProductCart productCart = new ProductCart();
            //    productCart.Products = products;
            //    productCart.CartUser = user;
            //    productCart.UerID = user.Id;
            //    productCart.Qty = buyQty.Value;

            //    return View("~/Views/Products/Cart.cshtml", productCart);
            //}


            var productListJson = HttpContext.Session.GetString("Cart");
            ViewBag.Cart = HttpContext.Session.GetString("Cart");
            if (!string.IsNullOrEmpty(productListJson))
            {
                decimal totalProduct = 0;
                decimal subtotal = 0;
                var productList = JsonConvert.DeserializeObject<List<ProductCart>>(productListJson);
                foreach(ProductCart productCart in productList )
                {
                    totalProduct = productCart.Qty * productCart.CartProduct.Price;
                    subtotal += totalProduct;
                }
                ViewBag.TotalProduct = totalProduct;
                ViewBag.Subtotal = subtotal;

                return View(productList);

            }
            return View(new List<ProductCart>());
        }
        public async Task<IActionResult> RemoveToCart(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var productListJson = HttpContext.Session.GetString("Cart");
            if (!string.IsNullOrEmpty(productListJson))
            {
                var productList = JsonConvert.DeserializeObject<List<ProductCart>>(productListJson);
                var productToRemove = productList.FirstOrDefault(p => p.CartProduct.Id == id);

                if (productToRemove != null)
                {
                    productList.Remove(productToRemove);
                    // Cập nhật Session với danh sách đã cập nhật
                    string updatedProductListJson = JsonConvert.SerializeObject(productList);
                    HttpContext.Session.SetString("Cart", updatedProductListJson);
                    HttpContext.Session.SetString("CartNumber", productList.Count.ToString());
                }
            }
            return RedirectToAction("Cart", "Products");
        }
        
        
        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            PopulateCategoriesDropDownList();
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Image,Price,Description,CategoryID")] Product product, List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            var filePaths = new List<string>();
            foreach (var formFile in files)
            {
                //Check if the file has a valid extension
                var fileExtension = Path.GetExtension(formFile.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Invalid file extension. Allowed extensions are: " + string.Join(",", allowedExtensions));
                }

                if (formFile.Length > 0)
                {
                    //change the folder path to where you want to store the upload files
                    var uploadFolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "customer/images/uploads");
                    Directory.CreateDirectory(uploadFolderPath);

                    var fileName = Path.GetRandomFileName() + fileExtension;
                    var filePath = Path.Combine(uploadFolderPath, fileName);
                    filePaths.Add(filePath);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }

            try
            {
                if (true)
                {
                    if (filePaths.Count > 0)
                    {
                        product.Image = "/customer/images/uploads/" + Path.GetFileName(filePaths[0]);
                    }
                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists " +
                    "see your system administrator.");
            }
            PopulateCategoriesDropDownList(product.CategoryID);
            //ViewData["CategoryID"] = new SelectList(_context.Category, "Id", "Name", product.CategoryID);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            PopulateCategoriesDropDownList(product.CategoryID);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id, List<IFormFile> files)
        {
            if (id == null)
            {
                return NotFound();
            }

            long size = files.Sum(f => f.Length);

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            var filePaths = new List<string>();
            foreach (var formFile in files)
            {
                //Check if the file has a valid extension
                var fileExtension = Path.GetExtension(formFile.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Invalid file extension. Allowed extensions are: " + string.Join(",", allowedExtensions));
                }

                if (formFile.Length > 0)
                {
                    //change the folder path to where you want to store the upload files
                    var uploadFolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "customer/images/uploads");
                    Directory.CreateDirectory(uploadFolderPath);

                    var fileName = Path.GetRandomFileName() + fileExtension;
                    var filePath = Path.Combine(uploadFolderPath, fileName);
                    filePaths.Add(filePath);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }


            var productToUpdate = await _context.Product
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (filePaths.Count > 0)
            {
                var Image = "/customer/images/uploads/" + Path.GetFileName(filePaths[0]);
            }

            if (await TryUpdateModelAsync<Product>(productToUpdate,
                "",
                p => p.Name,p => p.Image, p => p.Price, p => p.Description, p => p.CategoryID))
            {
                try
                {
                    //_context.Update(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }
            PopulateCategoriesDropDownList(productToUpdate.CategoryID);
            return View(productToUpdate);
        }

        private void PopulateCategoriesDropDownList(object selectedCategory = null)
        {
            var categoriesQuery = from d in _context.Category
                                  orderby d.Name
                                  select d;
            ViewBag.CategoryID = new SelectList(categoriesQuery.AsNoTracking(), "Id", "Name", selectedCategory);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product != null)
            {
                _context.Product.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.Id == id);
        }
    }
}
