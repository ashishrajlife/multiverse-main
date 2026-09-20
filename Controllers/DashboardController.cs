using ERPDemo.Data;
using ERPDemo.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPDemo.Controllers
{
    [Authorize]
    [NoCache]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("RoleName");

            return role switch
            {
                "SuperAdmin" => RedirectToAction(nameof(SuperAdminDashboard)),
                "Admin"      => RedirectToAction(nameof(AdminDashboard)),
                _            => RedirectToAction(nameof(UserDashboard))
            };
        }

        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> SuperAdminDashboard()
        {
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalAdmins = await _context.Users.Where(u => u.RoleId == 2).CountAsync();
            ViewBag.TotalSuperAdmins = await _context.Users.Where(u => u.RoleId == 3).CountAsync();
            ViewBag.RecentUsers = await _context.Users
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View();
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> AdminDashboard()
        {
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.ActiveUsers = await _context.Users.Where(u => u.IsActive).CountAsync();
            ViewBag.RecentUsers = await _context.Users
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View();
        }

        [Authorize(Roles = "User,Admin,SuperAdmin")]
        public async Task<IActionResult> UserDashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            ViewBag.User = user;
            return View();
        }
    }
}