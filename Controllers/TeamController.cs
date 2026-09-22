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

        // Role IDs: 1 = User, 2 = Manager, 3 = Admin, 4 = SuperAdmin
        private static readonly int[] AssignableRoleIds = { 1, 2 };

        // ============ INDEX / FALLBACK ============
        public IActionResult Index()
        {
            return RedirectToAction(nameof(ManageAll));
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

            ViewBag.Departments = await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
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

            ViewBag.Departments = await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
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
               Password = Guid.NewGuid().ToString(),
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                RoleId = model.RoleId,
                DepartmentId = model.DepartmentId,
                OrganizationId = orgId,
                 Address = model.Address,
            City = model.City,
            State = model.State,
            Pincode = model.Pincode,
            AadhaarNumber = model.AadhaarNumber,
            PanNumber = model.PanNumber,
            EmploymentType = model.EmploymentType,
            Designation = model.Designation,
            PlantLocation = model.PlantLocation,
            JoiningDate = model.JoiningDate,
            ShiftStartTime = model.ShiftStartTime,
            ShiftEndTime = model.ShiftEndTime,
            SalaryType = model.SalaryType,
            SalaryAmount = model.SalaryAmount,
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = $"'{model.Username}' team me add ho gaya.";
            return RedirectToAction(nameof(ManageAll));
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

            if (user.RoleId == 3 || user.RoleId == 4)
            {
                TempData["Error"] = "Aap Admin ya SuperAdmin ko edit nahi kar sakte.";
                return RedirectToAction(nameof(ManageAll));
            }

            ViewBag.Roles = await _context.Roles
                .Where(r => AssignableRoleIds.Contains(r.RoleId))
                .OrderBy(r => r.RoleId)
                .ToListAsync();

            ViewBag.Departments = await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync();

            var vm = new EditTeamMemberViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                RoleId = user.RoleId,
                DepartmentId = user.DepartmentId,
                IsActive = user.IsActive,
                Address = user.Address,
                AadhaarNumber = user.AadhaarNumber,
                City = user.City,
                Designation = user.Designation,
                EmploymentType = user.EmploymentType,
                JoiningDate = user.JoiningDate,
                PanNumber = user.PanNumber,
                Pincode = user.Pincode,
                PlantLocation = user.PlantLocation,
                SalaryAmount =user.SalaryAmount,
                SalaryType = user.SalaryType,
                ShiftStartTime = user.ShiftStartTime,
                ShiftEndTime = user.ShiftEndTime,
                State = user.State
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
                return RedirectToAction(nameof(ManageAll));
            }

            ViewBag.Roles = await _context.Roles
                .Where(r => AssignableRoleIds.Contains(r.RoleId))
                .OrderBy(r => r.RoleId)
                .ToListAsync();

            ViewBag.Departments = await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
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
            user.DepartmentId = model.DepartmentId;
            user.IsActive = model.IsActive;
            user.UpdatedAt = DateTime.Now;
            user.Address = model.Address;
user.City = model.City;
user.State = model.State;
user.Pincode = model.Pincode;
user.AadhaarNumber = model.AadhaarNumber;
user.PanNumber = model.PanNumber;
user.EmploymentType = model.EmploymentType;
user.Designation = model.Designation;
user.PlantLocation = model.PlantLocation;
user.JoiningDate = model.JoiningDate;
user.ShiftStartTime = model.ShiftStartTime;
user.ShiftEndTime = model.ShiftEndTime;
user.SalaryType = model.SalaryType;
user.SalaryAmount = model.SalaryAmount;

            await _context.SaveChangesAsync();
            TempData["Success"] = "User details update ho gayi.";
            return RedirectToAction(nameof(ManageAll));
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
            return RedirectToAction(nameof(ManageAll));
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
                return RedirectToAction(nameof(ManageAll));
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id && u.OrganizationId == orgId);
            if (user == null) return NotFound();

            if (user.RoleId == 3 || user.RoleId == 4)
            {
                TempData["Error"] = "Aap is user ko delete nahi kar sakte.";
                return RedirectToAction(nameof(ManageAll));
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
                .Where(u => u.RoleId != 3 && u.RoleId != 4)  // Admin and SuperAdmin excluded from list
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

            if (!string.IsNullOrWhiteSpace(department) && int.TryParse(department, out var deptId))
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
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    Address = u.Address,
                    City = u.City,
                    State = u.State,
                    Pincode = u.Pincode,
                    AadhaarNumber = u.AadhaarNumber,
                    PanNumber = u.PanNumber,
                    EmploymentType = u.EmploymentType,
                    Designation = u.Designation,
                    PlantLocation = u.PlantLocation,
                    JoiningDate = u.JoiningDate,
                    ShiftStartTime = u.ShiftStartTime,
                    ShiftEndTime = u.ShiftEndTime,
                    SalaryType = u.SalaryType,
                    SalaryAmount = u.SalaryAmount
                })
                .ToListAsync();

            var vm = new ManageAllViewModel
            {
                Members = members,
                Departments = await _context.Departments
                    .Where(d => d.IsActive)
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
    }
}