using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CoffeeLands.Data;
using CoffeeLands.Models;
using CoffeeLands.Helpers;
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
                    user.Password = DataEncryptionExtensions.HashPassword(user.Password);
                    _context.Add(user);
                    await _context.SaveChangesAsync();

                    var data = new SendMailRequest
                    {
                        ToEmail = user.Email,
                        UserName = user.Name,
                        Url = "verify"
                    };
                    await _mailService.SendBodyEmailAsync(data);
                    TempData["Notification"] = "Account created successfully, please check your email to activate your account.";
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
                var myUser = _context.User.SingleOrDefault(u => u.Email == user.Email && u.Password == DataEncryptionExtensions.HashPassword(user.Password));
                if (myUser == null)
                {
                    ModelState.AddModelError(string.Empty, "Email or password is incorrect!");
                }
                else
                {
                    if (myUser.Is_active == false)
                    {
                        ModelState.AddModelError(string.Empty, "Your account has not been activated, please check your email!");
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
            HttpContext.Session.Remove("Cart");
            return Redirect("/");
        }
        #endregion

        #region forgot password
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View("~/Views/Home/Account/ForgotPassword.cshtml");
        }
        [HttpPost]
        public async Task<IActionResult> SendEmailForgotPassword(string email)
        {
            if(email == null)
            {
                ModelState.AddModelError(string.Empty, "Email is required!");
                return View("~/Views/Home/Account/ForgotPassword.cshtml");
            }
            var accounts = await _context.User
                .SingleOrDefaultAsync(a => a.Email == email);
            
            if(accounts != null)
            {
                var data = new SendMailRequest
                {
                    ToEmail = accounts.Email,
                    UserName = accounts.Name,
                    Url = "ForgotPassword"
                };
                await _mailService.SendBodyEmailAsync(data);
                TempData["Notification"] = "The password reset link has been sent to your email, please check your email!";
                return Redirect("ForgotPassword");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Email does not exist!");
            }

            return View("~/Views/Home/Account/ForgotPassword.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string email)
        {
            var accountReset = await _context.User
                .SingleOrDefaultAsync(a => a.Email == email);

            if (accountReset != null)
            {
                return View("~/Views/Home/Account/ResetPassword.cshtml",accountReset);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Email does not exist!");
                return View("~/Views/Home/Account/ForgotPassword.cshtml");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordPost(int? id, string pwd, string confirmpwd)
        {
            if(id == null)
            {
                return NotFound();
            }
            var account = await _context.User.SingleOrDefaultAsync(a => a.Id == id);
            if(account != null)
            {
                if(pwd != confirmpwd)
                {
                    ModelState.AddModelError(string.Empty, "Confirm Password and Password are not the same.");
                }
                else
                {
                    account.Password = DataEncryptionExtensions.HashPassword(pwd);
                    await _context.SaveChangesAsync();
                    return Redirect("Login");
                }
            }

            return View("~/Views/Home/Account/ResetPassword.cshtml");
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
