using System.Security.Claims;
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
    var role = HttpContext.Session.GetString("RoleName")
               ?? User.FindFirst(ClaimTypes.Role)?.Value;

    // Safety — agar role hi nahi hai toh login pe bhejo
    if (string.IsNullOrEmpty(role))
    {
        return RedirectToAction("Login", "Auth");
    }

    return role switch
    {
        "Admin"      => RedirectToAction(nameof(AdminDashboard)),
        "Manager"    => RedirectToAction(nameof(AdminDashboard)),
        _            => RedirectToAction(nameof(UserDashboard))
    };
}
        // Helper class — controller ke andar hi rakh sakte ho ya ViewModels mein
        public class CityStat
        {
            public string City { get; set; } = string.Empty;
            public int Count { get; set; }
        }

[Authorize(Roles = "Admin,Manager")]
public async Task<IActionResult> AdminDashboard()
        {
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.ActiveUsers = await _context.Users.Where(u => u.IsActive).CountAsync();
            ViewBag.RecentUsers = await _context.Users
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

                ViewBag.PendingApprovalsCount = await _context.Requisitions
                .Where(r => r.Status == "PendingHOD")
                .CountAsync();

            return View();
        }

        [Authorize(Roles = "User,Manager,Admin")]
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