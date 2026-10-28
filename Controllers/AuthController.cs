using System.Security.Claims;
using ERPDemo.Data;
using ERPDemo.Services;
using ERPDemo.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPDemo.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _context; // 👈 NAYA: DB inject kiya permissions ke liye

        public AuthController(IAuthService authService, AppDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Sirf tab redirect karo jab cookie AUTHENTIC ho
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                if (!string.IsNullOrEmpty(role))
                {
                    try
                    {
                        return role switch
                        {
                            "Admin"   => RedirectToAction("AdminDashboard", "Dashboard"),
                            "Manager" => RedirectToAction("AdminDashboard", "Dashboard"), // Temp: Manager bhi Admin dashboard dekhega (baad mein Manager dashboard banayenge)
                            "HOD"     => RedirectToAction("AdminDashboard", "Dashboard"), // Temp: HOD bhi Admin dashboard dekhega
                            _         => RedirectToAction("Login", "Auth")
                        };
                    }
                    catch
                    {
                        return View();
                    }
                }
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _authService.ValidateUserAsync(model.UsernameOrEmail, model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            await _authService.UpdateLastLoginAsync(user.UserId);

            var roleName = user.Role?.RoleName ?? "User";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("FullName", user.FullName ?? user.Username)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(60)
                });

            var token = _authService.GenerateJwtToken(user);
            Response.Cookies.Append("JWToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.Now.AddMinutes(60)
            });

            // ═══ SESSION SETUP ═══
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("RoleName", roleName); // 👈 Ye miss ho gaya tha
            HttpContext.Session.SetString("FullName", user.FullName ?? user.Username);

            // ═══ FETCH & SAVE PERMISSIONS ═══
            // Admin ke paas hamesha sab access hota hai, baaki ke liye DB se fetch karo
            if (roleName == "Admin")
            {
                HttpContext.Session.SetString("UserPermissions", "Dashboard,Vehicles,Drivers,Trips,Parties,FuelLogs,Maintenance,Reports,Team");
            }
            else
            {
                var perms = await _context.RolePermissions
                    .Where(rp => rp.RoleId == user.RoleId)
                    .Select(rp => rp.ModuleName)
                    .ToListAsync();

                HttpContext.Session.SetString("UserPermissions", string.Join(",", perms));
            }

            // ═══ REDIRECT ═══
            return roleName switch
            {
                "Admin"   => RedirectToAction("AdminDashboard", "Dashboard"),
                "Manager" => RedirectToAction("ManagerDashboard", "Dashboard"),
                "HOD"     => RedirectToAction("HodDashboard", "Dashboard"),
                _         => RedirectToAction("Login", "Auth")
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("JWToken");
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}