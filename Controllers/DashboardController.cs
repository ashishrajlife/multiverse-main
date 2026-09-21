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
            // Try both: session aur claims se role nikalo
            var role = HttpContext.Session.GetString("RoleName");

            // Fallback: agar session empty hai toh claims se lo
            if (string.IsNullOrEmpty(role))
            {
                role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            }

            return role switch
            {
                "SuperAdmin" => RedirectToAction(nameof(SuperAdminDashboard)),
                "Admin"      => RedirectToAction(nameof(AdminDashboard)),
                "Manager"    => RedirectToAction(nameof(AdminDashboard)),
                _            => RedirectToAction(nameof(UserDashboard))
            };
        }

       [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> SuperAdminDashboard()
        {
            // ===== RECENT ORGANIZATIONS (Last 5) =====
            ViewBag.RecentOrganizations = await _context.Organizations
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            // ===== ORGANIZATION GROWTH (Last 6 months) =====
            var sixMonthsAgo = DateTime.Now.AddMonths(-5);
            var startOfMonth = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

            var orgsByMonth = await _context.Organizations
                .Where(o => o.CreatedAt >= startOfMonth)
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync();

            var monthLabels = new List<string>();
            var monthCounts = new List<int>();
            for (int i = 0; i < 6; i++)
            {
                var m = startOfMonth.AddMonths(i);
                monthLabels.Add(m.ToString("MMM yyyy"));
                var match = orgsByMonth.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month);
                monthCounts.Add(match?.Count ?? 0);
            }
            ViewBag.MonthLabels = monthLabels;
            ViewBag.MonthCounts = monthCounts;

            return View();
        }
        // Helper class — controller ke andar hi rakh sakte ho ya ViewModels mein
        public class CityStat
        {
            public string City { get; set; } = string.Empty;
            public int Count { get; set; }
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