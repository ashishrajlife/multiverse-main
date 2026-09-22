using ERPDemo.Data;
using ERPDemo.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ERPDemo.Controllers
{
    [Authorize(Roles = "Manager")]
    [NoCache]
    public class ManagerController : Controller
    {
        private readonly AppDbContext _context;

        public ManagerController(AppDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        public async Task<IActionResult> Dashboard()
        {
            var manager = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

            if (manager == null) return RedirectToAction("Login", "Auth");

            // Manager ke department ke employees
            var teamMembers = await _context.Users
                .Where(u => u.OrganizationId == manager.OrganizationId 
                         && u.DepartmentId == manager.DepartmentId
                         && u.RoleId == 1)
                .ToListAsync();

            ViewBag.ManagerName = manager.FullName ?? manager.Username;
            ViewBag.DepartmentName = manager.Department?.Name ?? "Unassigned";
            ViewBag.TeamCount = teamMembers.Count;
            ViewBag.ActiveCount = teamMembers.Count(m => m.IsActive);
            ViewBag.InactiveCount = teamMembers.Count(m => !m.IsActive);
            ViewBag.TeamMembers = teamMembers;

            return View();
        }
    }
}