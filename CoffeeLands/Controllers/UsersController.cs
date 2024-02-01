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
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace CoffeeLands.Controllers
{
    public class UsersController : Controller
    {
        private readonly CoffeeLandsContext _context;

        public UsersController(CoffeeLandsContext context)
        {
            _context = context;

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
                    _context.Add(user);
                    await _context.SaveChangesAsync();
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
        public IActionResult Login(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return View("~/Views/Home/Account/Login.cshtml");
        }
        [HttpPost]
        public async Task<IActionResult> Login(User user, string? ReturnUrl)
        {
            //ViewBag.ReturnUrl = ReturnUrl;
            //         if (true)
            //         {
            //             var myUser = _context.User.SingleOrDefault(u => u.Email == user.Email && u.Password == user.Password);
            //             if(myUser == null)
            //             {
            //                 ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            //             }
            //             else
            //             {
            //                 if(myUser.Email != user.Email || myUser.Password != user.Password)
            //                 {
            //                     ModelState.AddModelError("Error!", "Tài khoản hoặc mật khẩu không đúng!");
            //                 }
            //                 else
            //                 {
            //                     var claims = new List<Claim>
            //                     {
            //                         new Claim(ClaimTypes.Email, myUser.Email),
            //                         new Claim(ClaimTypes.Name, myUser.Name),
            //                         //new Claim("userID", user.Id)

            //                         new Claim(ClaimTypes.Role, myUser.Role)

            //                     };

            //                     var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            //                     var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            //                     await HttpContext.SignInAsync(claimsPrincipal);

            //                     HttpContext.Session.SetString("UserSession", myUser.Name);
            //                     HttpContext.Session.SetInt32("UserId", myUser.Id);



            //                     if (myUser.Role == "ADMIN")
            //                     {
            //                         HttpContext.Session.SetString("Admin", myUser.Name);
            //                     }

            //                     if (Url.IsLocalUrl(ReturnUrl))
            //                     {
            //                         return Redirect(ReturnUrl);
            //                     }
            //                     else
            //                     {
            //                         return Redirect("/");
            //                     }

            //                 }
            //             }
            //         }

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
                    else
                    {
                        HttpContext.Session.SetString("UserSession", myUser.Name);
                        HttpContext.Session.SetInt32("UserId", myUser.Id);
                        if (myUser.Role == "ADMIN")
                        {
                            HttpContext.Session.SetString("Admin", myUser.Name);
                        }
                        return Redirect("/");
                    }
                }
            }
            return View("~/Views/Home/Account/Login.cshtml");
        }

        public async Task<IActionResult> Logout()
        {
            //await HttpContext.SignOutAsync();
            if (HttpContext.Session.GetString("UserSession") != null)
            {
                HttpContext.Session.Remove("UserSession");
                return RedirectToAction("Index", "Home");
            }
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
