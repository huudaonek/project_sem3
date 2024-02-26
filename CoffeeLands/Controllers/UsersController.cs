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
using System.Security.Cryptography;
using System.Text;
using CoffeeLands.Services;
using CoffeeLands.ViewModels.Mail;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;


namespace CoffeeLands.Controllers
{
    public class UsersController : Controller
    {
        private readonly CoffeeLandsContext _context;
        private readonly IMailService _mailService;

        public UsersController(CoffeeLandsContext context, IMailService mailService)
        {
            _context = context;
            _mailService = mailService;
        }

        // Index Users
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

            var users = from u in _context.User
                        select u;
            if (!String.IsNullOrEmpty(searchString))
            {
                users = users.Where(s => s.Name.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    users = users.OrderByDescending(s => s.Name);
                    break;
                default:
                    users = users.OrderBy(s => s.Name);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<User>.CreateAsync(users.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        //Detail User
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


        #region update
        //public IActionResult Register()
        //{
        //    return View("~/Views/Home/Account/Register.cshtml");
        //}
        //public IActionResult Login()
        //{
        //    if (HttpContext.Session.GetString("UserSession") != null)
        //    {
        //        return RedirectToAction("Index", "Home");
        //    }
        //    return View("~/Views/Home/Account/Login.cshtml");
        //}
        //[HttpPost]
        //public async Task<IActionResult> Login(User model)
        //{
        //    var user = await _context.User.FirstOrDefaultAsync(u => u.Email == model.Email);

        //    if (user != null && VerifyPassword(model.Password, user.Password))
        //    {
        //        HttpContext.Session.SetString("UserSession", JsonConvert.SerializeObject(user));
        //        ViewBag.MySession = user.Name;
        //        HttpContext.Session.SetInt32("UserId", user.Id);
        //        if (user.Role == "ADMIN")
        //        {
        //            HttpContext.Session.SetString("Admin", user.Name);
        //        }
        //        return RedirectToAction("Index", "Home");
        //    }
        //    else
        //    {
        //        ModelState.AddModelError(string.Empty, "Invalid email or password");
        //        return View("~/Views/Home/Account/Login.cshtml");
        //    }
        //}

        //// Hàm xác minh mật khẩu
        //private bool VerifyPassword(string inputPassword, string hashedPassword)
        //{
        //    string hashedInputPassword = HashPassword(inputPassword);

        //    return hashedInputPassword == hashedPassword;
        //}


        //public IActionResult Logout()
        //{
        //    if (HttpContext.Session.GetString("UserSession") != null)
        //    {
        //        HttpContext.Session.Remove("UserSession");
        //        return RedirectToAction("Index", "Home");
        //    }
        //    return View("~/Views/Home/Pages/Index.cshtml");
        //}




        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Name,Email,Password,Role")] User user)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        user.Password = HashPassword(user.Password);
        //        user.Role = "USER";
        //        _context.User.Add(user);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction("Index", "Home");
        //    }
        //    return View(user);
        //}
        //private string HashPassword(string password)
        //{
        //    using (SHA256 sha256Hash = SHA256.Create())
        //    {
        //        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

        //        // Chuyển đổi mảng byte thành chuỗi và chọn một phần của chuỗi để sử dụng
        //        string hashedPassword = BitConverter.ToString(bytes).Replace("-", "").Substring(0, 29);

        //        return hashedPassword;
        //    }
        //}
        #endregion


        #region Register
        public IActionResult Register()
        {
            return View("~/Views/Home/Account/Register.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Email,Password")] User user)
        {
            try
            {
                if (true)
                {
                    user.Role = "CUSTOMER";
                    _context.Add(user);
                    await _context.SaveChangesAsync();

                    var data = new SendMailRequest
                    {
                        ToEmail = user.Email,
                        UserName = user.Name,
                        Url = "verify"
                    };
                    await _mailService.SendBodyEmailAsync(data);

                    return RedirectToAction("Login", "Users");
                }
            }
            catch (DbUpdateException /* ex */)
            {
                //Log the error (uncomment ex variable name and write a log.
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists " +
                    "see your system administrator.");
            }
            return View("~/Views/Home/Account/Register.cshtml", user);
        }
        #endregion
        #region Login
        [HttpGet]
        public IActionResult Login(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View("~/Views/Home/Account/Login.cshtml");
        }
        [HttpPost]
        public async Task<IActionResult> Login(User user, string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if (true)
            {
                var myUser = _context.User.SingleOrDefault(u => u.Email == user.Email && u.Password == user.Password);
                if (myUser == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
                else
                {
                    if (myUser.Email != user.Email || myUser.Password != user.Password)
                    {
                        ModelState.AddModelError("Error!", "Tài khoản hoặc mật khẩu không đúng!");
                    } 
                    else if (myUser.Is_active == false)
                    {
                        ModelState.AddModelError("Error!", "Tài khoản của bạn chưa được kích hoạt, vui lòng check lại email để kích hoạt!");
                    }
                    else
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Email, myUser.Email),
                            new Claim(ClaimTypes.Name, myUser.Name),
                            new Claim("CustomerID", myUser.Id.ToString()),
                            //claim - role động
                            new Claim(ClaimTypes.Role, myUser.Role)

                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await HttpContext.SignInAsync(claimsPrincipal);
                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return Redirect("/");
                        }
                        
                    }
                }
            }
            return View("~/Views/Home/Account/Login.cshtml");
        }
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
                
            
            return Redirect("/");
        }
        #endregion

        #region Edit User
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Password,Role")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }
        #endregion
        #region Delete User
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user != null)
            {
                _context.User.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion
        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}
