using ERPDemo.Data;
using ERPDemo.Filters;
using ERPDemo.Models;
using ERPDemo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ERPDemo.Controllers
{
    [Authorize(Roles = "Admin")]
    [NoCache]
    public class TeamController : Controller
    {
        private readonly AppDbContext _context;

        public TeamController(AppDbContext context)
        {
            _context = context;
        }

        // ============ HELPERS ============
        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        private async Task<int?> GetAdminOrgIdAsync()
        {
            var admin = await _context.Users.FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
            return admin?.OrganizationId;
        }

        // Role IDs (seed se):
        // 1 = User, 2 = Manager, 3 = Admin, 4 = SuperAdmin
        private static readonly int[] AssignableRoleIds = { 1, 2 };

        // ============ LIST ============
        public async Task<IActionResult> Index(string? search, string? role, string? status)
        {
            var orgId = await GetAdminOrgIdAsync();
            if (orgId == null)
            {
                TempData["Error"] = "Aap kisi organization se attached nahi hai.";
                return RedirectToAction("AdminDashboard", "Dashboard");
            }

            var query = _context.Users
                .Include(u => u.Role)
                .Where(u => u.OrganizationId == orgId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(u =>
                    u.Username.ToLower().Contains(s) ||
                    u.Email.ToLower().Contains(s) ||
                    (u.FullName != null && u.FullName.ToLower().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(u => u.Role!.RoleName == role);

            if (status == "active") query = query.Where(u => u.IsActive);
            else if (status == "inactive") query = query.Where(u => !u.IsActive);

            var members = await query
                .OrderBy(u => u.RoleId).ThenBy(u => u.Username)
                .Select(u => new TeamRow
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    RoleName = u.Role!.RoleName,
                    RoleId = u.RoleId,
                    IsActive = u.IsActive,
                    LastLoginAt = u.LastLoginAt,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            var vm = new TeamViewModel
            {
                Members = members,
                SearchTerm = search,
                RoleFilter = role,
                StatusFilter = status,
                TotalMembers = members.Count,
                ActiveMembers = members.Count(m => m.IsActive),
                ManagerCount = members.Count(m => m.RoleName == "Manager"),
                UserCount = members.Count(m => m.RoleName == "User")
            };

            return View(vm);
        }

        // ============ CREATE ============
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var orgId = await GetAdminOrgIdAsync();
            if (orgId == null) return RedirectToAction("AdminDashboard", "Dashboard");

            ViewBag.Roles = await _context.Roles
                .Where(r => AssignableRoleIds.Contains(r.RoleId))
                .OrderBy(r => r.RoleId)
                .ToListAsync();

            return View(new CreateTeamMemberViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTeamMemberViewModel model)
        {
            var orgId = await GetAdminOrgIdAsync();
            if (orgId == null) return RedirectToAction("AdminDashboard", "Dashboard");

            ViewBag.Roles = await _context.Roles
                .Where(r => AssignableRoleIds.Contains(r.RoleId))
                .OrderBy(r => r.RoleId)
                .ToListAsync();

            if (!ModelState.IsValid) return View(model);

            // Role check
            if (!AssignableRoleIds.Contains(model.RoleId))
            {
                ModelState.AddModelError("RoleId", "Aap sirf User ya Manager assign kar sakte hai.");
                return View(model);
            }

            // Duplicate checks
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Ye username already exist karta hai.");
                return View(model);
            }
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Ye email already registered hai.");
                return View(model);
            }

            _context.Users.Add(new User
            {
                Username = model.Username,
                Email = model.Email,
                Password = model.Password,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                RoleId = model.RoleId,
                OrganizationId = orgId,
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = $"'{model.Username}' team me add ho gaya.";
            return RedirectToAction(nameof(Index));
        }

        // ============ EDIT ============
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var orgId = await GetAdminOrgIdAsync();
            if (orgId == null) return RedirectToAction("AdminDashboard", "Dashboard");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id && u.OrganizationId == orgId);
            if (user == null) return NotFound();

            // Admin/Manager edit kar sakta hai, Admin/SuperAdmin ko nahi
            if (user.RoleId == 3 || user.RoleId == 4)
            {
                TempData["Error"] = "Aap Admin ya SuperAdmin ko edit nahi kar sakte.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = await _context.Roles
                .Where(r => AssignableRoleIds.Contains(r.RoleId))
                .OrderBy(r => r.RoleId)
                .ToListAsync();

            var vm = new EditTeamMemberViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                RoleId = user.RoleId,
                IsActive = user.IsActive
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditTeamMemberViewModel model)
        {
            var orgId = await GetAdminOrgIdAsync();
            if (orgId == null) return RedirectToAction("AdminDashboard", "Dashboard");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == model.UserId && u.OrganizationId == orgId);
            if (user == null) return NotFound();

            if (user.RoleId == 3 || user.RoleId == 4)
            {
                TempData["Error"] = "Aap Admin ya SuperAdmin ko edit nahi kar sakte.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = await _context.Roles
                .Where(r => AssignableRoleIds.Contains(r.RoleId))
                .OrderBy(r => r.RoleId)
                .ToListAsync();

            if (!ModelState.IsValid) return View(model);

            if (!AssignableRoleIds.Contains(model.RoleId))
            {
                ModelState.AddModelError("RoleId", "Aap sirf User ya Manager assign kar sakte hai.");
                return View(model);
            }

            // Duplicate checks
            if (await _context.Users.AnyAsync(u => u.Username == model.Username && u.UserId != model.UserId))
            {
                ModelState.AddModelError("Username", "Ye username already exist karta hai.");
                return View(model);
            }
            if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.UserId != model.UserId))
            {
                ModelState.AddModelError("Email", "Ye email already registered hai.");
                return View(model);
            }

            user.Username = model.Username;
            user.Email = model.Email;
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.RoleId = model.RoleId;
            user.IsActive = model.IsActive;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Success"] = "User details update ho gayi.";
            return RedirectToAction(nameof(Index));
        }

        // ============ TOGGLE ACTIVE ============
       [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ToggleActive(
    int id,
    string? search = null,
    string? role = null,
    string? department = null,
    string? status = null)
{
    // Helper to rebuild the redirect with filters preserved
    IActionResult BackToManageAll() =>
        RedirectToAction(nameof(ManageAll), new { search, role, department, status });

    var orgId = await GetAdminOrgIdAsync();
    if (orgId == null) return RedirectToAction("AdminDashboard", "Dashboard");

    if (id == CurrentUserId)
    {
        TempData["Error"] = "Aap khud ko deactivate nahi kar sakte.";
        return BackToManageAll();
    }

    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.UserId == id && u.OrganizationId == orgId);
    if (user == null) return NotFound();

    if (user.RoleId == 3 || user.RoleId == 4)
    {
        TempData["Error"] = "Aap is user ki status change nahi kar sakte.";
        return BackToManageAll();
    }

    user.IsActive = !user.IsActive;
    user.UpdatedAt = DateTime.Now;
    await _context.SaveChangesAsync();

    TempData["Success"] = $"'{user.Username}' {(user.IsActive ? "activate" : "deactivate")} ho gaya.";
    return BackToManageAll();
}

        // ============ RESET PASSWORD ============
        [HttpGet]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var orgId = await GetAdminOrgIdAsync();
            if (orgId == null) return RedirectToAction("AdminDashboard", "Dashboard");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id && u.OrganizationId == orgId);
            if (user == null) return NotFound();

            ViewBag.UserName = user.FullName ?? user.Username;
            return View(new ResetPasswordViewModel { UserId = user.UserId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var orgId = await GetAdminOrgIdAsync();
            if (orgId == null) return RedirectToAction("AdminDashboard", "Dashboard");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == model.UserId && u.OrganizationId == orgId);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.UserName = user.FullName ?? user.Username;
                return View(model);
            }

            user.Password = model.NewPassword;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{user.Username}' ka password reset ho gaya.";
            return RedirectToAction(nameof(Index));
        }

        // ============ DELETE ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id,
            string? search = null,
            string? role = null,
            string? department = null,
            string? status = null)
        {
            var orgId = await GetAdminOrgIdAsync();
            if (orgId == null) return RedirectToAction("AdminDashboard", "Dashboard");

            if (id == CurrentUserId)
            {
                TempData["Error"] = "Aap khud ko delete nahi kar sakte.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id && u.OrganizationId == orgId);
            if (user == null) return NotFound();

            if (user.RoleId == 3 || user.RoleId == 4)
            {
                TempData["Error"] = "Aap is user ko delete nahi kar sakte.";
                return RedirectToAction(nameof(Index));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{user.Username}' delete ho gaya.";
    return RedirectToAction(nameof(ManageAll), new { search, role, department, status });
}

        // ============ MANAGE ALL (Super Table) ============
        [HttpGet]
        public async Task<IActionResult> ManageAll(string? search, string? role, string? department, string? status)
        {
            var orgId = await GetAdminOrgIdAsync();
            if (orgId == null)
            {
                TempData["Error"] = "Aap kisi organization se attached nahi hai.";
                return RedirectToAction("AdminDashboard", "Dashboard");
            }

            var query = _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Where(u => u.OrganizationId == orgId)
                .Where(u => u.RoleId != 3 && u.RoleId != 4)  // Admin aur SuperAdmin exclude
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(u =>
                    u.Username.ToLower().Contains(s) ||
                    u.Email.ToLower().Contains(s) ||
                    (u.FullName != null && u.FullName.ToLower().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                if (role == "manager") query = query.Where(u => u.RoleId == 2);
                else if (role == "employee") query = query.Where(u => u.RoleId == 1);
            }

            if (department.HasValue() && int.TryParse(department, out var deptId))
                query = query.Where(u => u.DepartmentId == deptId);

            if (status == "active") query = query.Where(u => u.IsActive);
            else if (status == "inactive") query = query.Where(u => !u.IsActive);

            var members = await query
                .OrderBy(u => u.RoleId).ThenBy(u => u.Username)
                .Select(u => new ManageRow
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    RoleName = u.Role!.RoleName,
                    RoleId = u.RoleId,
                    DepartmentId = u.DepartmentId,
                    DepartmentName = u.Department != null ? u.Department.Name : null,
                    IsActive = u.IsActive,
                    LastLoginAt = u.LastLoginAt,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            var vm = new ManageAllViewModel
            {
                Members = members,
                Departments = await _context.Departments
                    .Where(d => d.OrganizationId == orgId && d.IsActive)
                    .OrderBy(d => d.Name)
                    .Select(d => new DepartmentOption { DepartmentId = d.DepartmentId, Name = d.Name })
                    .ToListAsync(),
                SearchTerm = search,
                RoleFilter = role,
                DepartmentFilter = department,
                StatusFilter = status,
                TotalMembers = members.Count,
                ManagerCount = members.Count(m => m.RoleName == "Manager"),
                EmployeeCount = members.Count(m => m.RoleName == "User"),
                ActiveCount = members.Count(m => m.IsActive)
            };

            return View(vm);
        }

        // ============ TOGGLE ROLE (AJAX) ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleRole([FromBody] ToggleRoleRequest request)
        {
            var orgId = await GetAdminOrgIdAsync();
            if (orgId == null)
                return Json(new { success = false, error = "No organization attached." });

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == request.UserId && u.OrganizationId == orgId);

            if (user == null)
                return Json(new { success = false, error = "User not found." });

            // Safety: Admin aur SuperAdmin ko touch nahi karna
            if (user.RoleId == 3 || user.RoleId == 4)
                return Json(new { success = false, error = "Admin aur SuperAdmin ka role change nahi ho sakta." });

            // Sirf User (1) ↔ Manager (2) allowed
            if (user.RoleId != 1 && user.RoleId != 2)
                return Json(new { success = false, error = "Ye role change nahi ho sakta." });

            // Toggle
            user.RoleId = user.RoleId == 1 ? 2 : 1;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            var roleName = user.RoleId == 2 ? "Manager" : "Employee";
            return Json(new
            {
                success = true,
                newRoleId = user.RoleId,
                newRoleName = roleName,
                message = $"{user.Username} is now a {roleName}."
            });
        }

        // ============ UPDATE DEPARTMENT (AJAX) ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDepartment([FromBody] UpdateDepartmentRequest request)
        {
            var orgId = await GetAdminOrgIdAsync();
            if (orgId == null)
                return Json(new { success = false, error = "No organization attached." });

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == request.UserId && u.OrganizationId == orgId);
            if (user == null)
                return Json(new { success = false, error = "User not found." });

            if (user.RoleId == 3 || user.RoleId == 4)
                return Json(new { success = false, error = "Admin ka department change nahi ho sakta." });

            // Validate department belongs to same org
            if (request.DepartmentId.HasValue)
            {
                var deptExists = await _context.Departments
                    .AnyAsync(d => d.DepartmentId == request.DepartmentId && d.OrganizationId == orgId);
                if (!deptExists)
                    return Json(new { success = false, error = "Department valid nahi hai." });
            }

            user.DepartmentId = request.DepartmentId;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Department update ho gaya." });
        }

        // DTOs
        public class ToggleRoleRequest
        {
            public int UserId { get; set; }
        }

        public class UpdateDepartmentRequest
        {
            public int UserId { get; set; }
            public int? DepartmentId { get; set; }
        }



    }

}