using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CoffeeLands.Data;
using CoffeeLands.Models;
using CoffeeLands.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using CoffeeLands.Helpers;
using PayPal.Api;
using Microsoft.CodeAnalysis;

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
        public List<CartItem> MyCart => HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();

        // Index Products
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
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

            int pageSize = 10;
            return View(await PaginatedList<Product>.CreateAsync(products.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        //Product Detail Admin
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

        //Product Detail Customer
        [HttpGet("ProductDetail/{id}")]
        public async Task<IActionResult> ProductDetail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.Category)
                .Include(f => f.Feedbacks)
                .ThenInclude(u => u.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        //Feedback

        public async Task<IActionResult> Feedback(int? idProduct, int? star)
        {
            if (idProduct == null)
            {
                return NotFound();
            }

            var feedbacks = await _context.Feedback
                                .Include(u => u.User)
                .Include(p => p.Product)
                .ThenInclude(c => c.Category)
                .ToListAsync();
            List<FeedbackVM> feedbackVMList = new List<FeedbackVM>();
            
            foreach (var feedback in feedbacks)
            {
                var item = new FeedbackVM
                {
                    Id = feedback.Id,
                    Vote = feedback.Vote,
                    imagesFeedback = feedback.imagesFeedback,
                    Description = feedback.Description,
                    UserName = feedback.User.Name,
                    UserImage = "~/wwwroot/customer/images/feedbacks/anh-1.jpg",
                    ProductId = feedback.Product.Id,
                    ProductName = feedback.Product.Name
                };
                feedbackVMList.Add(item);
            }

            var query = feedbackVMList.Where(f => f.ProductId == idProduct);

            if (star != null && star <= 5)
            {
                query = query.Where(f => f.Vote == star);
            }

            var result = query.ToArray();

            return Ok(result);
        }

        #region Create Product
        public IActionResult Create()
        {
            PopulateCategoriesDropDownList();
            return View();
        }

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
                        product.Image = "customer/images/uploads/" + Path.GetFileName(filePaths[0]);
                    }
                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException /* ex */)
            {
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists " +
                    "see your system administrator.");
            }
            PopulateCategoriesDropDownList(product.CategoryID);
            return View(product);
        }
        #endregion

        #region Edit Product
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

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id, List<IFormFile> files)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productToUpdate = await _context.Product
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (productToUpdate != null)
            {
                if (files != null && files.Count > 0)
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
                    productToUpdate.Image = "customer/images/uploads/" + Path.GetFileName(filePaths[0]);
                }
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
        #endregion
        private void PopulateCategoriesDropDownList(object selectedCategory = null)
        {
            var categoriesQuery = from d in _context.Category
                                  orderby d.Name
                                  select d;
            ViewBag.CategoryID = new SelectList(categoriesQuery.AsNoTracking(), "Id", "Name", selectedCategory);
        }

        #region Delete Product
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
        #endregion
        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.Id == id);
        }
    }
}
