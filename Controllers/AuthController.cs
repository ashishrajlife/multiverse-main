using System.Security.Claims;
using ERPDemo.Filters;
using ERPDemo.Services;
using ERPDemo.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace ERPDemo.Controllers
{
    [NoCache]   // apply to entire AuthController too
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Sirf tab redirect karo jab cookie AUTHENTIC ho
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                // Safety — agar role hi nahi mila toh login page dikhao
                if (!string.IsNullOrEmpty(role))
                {
                    try
                    {
                        return role switch
                        {
                            "SuperAdmin" => RedirectToAction("SuperAdminDashboard", "Dashboard"),
                            "Admin"      => RedirectToAction("AdminDashboard", "Dashboard"),
                            "HOD"        => RedirectToAction("Dashboard", "Hod"),  
                            "Manager"    => RedirectToAction("Dashboard", "Manager"),
                            _            => RedirectToAction("UserDashboard", "Dashboard")
                        };
                    }
                    catch
                    {
                        // Fallback — redirect fail hone pe login page dikhao
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

            if (user.RoleId == 1)
            {
                ModelState.AddModelError("", "Login access is restricted to Managers and Administrators only.");
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

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("RoleName", roleName);        
            HttpContext.Session.SetString("FullName", user.FullName ?? user.Username);

            return roleName switch
            {
                "SuperAdmin" => RedirectToAction("SuperAdminDashboard", "Dashboard"),
                "Admin"      => RedirectToAction("AdminDashboard", "Dashboard"),
               "Manager"    => RedirectToAction("Dashboard", "Manager"),
               "HOD"        => RedirectToAction("Dashboard", "Hod"),  
                _            => RedirectToAction("UserDashboard", "Dashboard")
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