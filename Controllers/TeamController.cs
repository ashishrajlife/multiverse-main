using ERPDemo.Data;
using ERPDemo.Models;
using ERPDemo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPDemo.Controllers
{
    [Authorize]
    public class TeamController : Controller
    {
        private readonly AppDbContext _context;

        public TeamController(AppDbContext context)
        {
            _context = context;
        }

        // LIST ALL USERS
        public async Task<IActionResult> ManageAll(string? search, int? roleId)
        {
            var query = _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u =>
                    u.Username.Contains(search) ||
                    u.Email.Contains(search) ||
                    (u.FullName != null && u.FullName.Contains(search)));

            if (roleId.HasValue)
                query = query.Where(u => u.RoleId == roleId.Value);

            var users = await query.OrderBy(u => u.FullName).ToListAsync();

            ViewBag.Search = search;
            ViewBag.RoleId = roleId;
            ViewBag.Roles = await _context.Roles.OrderBy(r => r.RoleName).ToListAsync();

            return View(users);
        }

        // CREATE GET
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _context.Roles.ToListAsync();
            ViewBag.Departments = await _context.Departments.Where(d => d.IsActive).ToListAsync();
            return View(new TeamUserFormViewModel());
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeamUserFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.Roles.ToListAsync();
                ViewBag.Departments = await _context.Departments.ToListAsync();
                return View(vm);
            }

            // Duplicate checks (Username & Email)
            var dupUser = await _context.Users.AnyAsync(u => u.Username == vm.Username);
            if (dupUser) ModelState.AddModelError(nameof(vm.Username), "Username already taken");
            var dupEmail = await _context.Users.AnyAsync(u => u.Email == vm.Email);
            if (dupEmail) ModelState.AddModelError(nameof(vm.Email), "Email already registered");

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.Roles.ToListAsync();
                ViewBag.Departments = await _context.Departments.ToListAsync();
                return View(vm);
            }

            // 1. Create User
            var user = new User
            {
                Username     = vm.Username,
                Email        = vm.Email,
                Password     = vm.Password,   
                FullName     = vm.FullName,
                PhoneNumber  = vm.PhoneNumber,
                RoleId       = vm.RoleId,
                DepartmentId = vm.DepartmentId,
                IsActive     = true,
                CreatedAt    = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 2. Save Permissions (Ye cheez add karni hai)
            if (vm.SelectedPermissions != null && vm.SelectedPermissions.Any())
            {
                foreach (var module in vm.SelectedPermissions)
                {
                    _context.RolePermissions.Add(new RolePermission 
                    { 
                        RoleId = vm.RoleId, 
                        ModuleName = module 
                    });
                }
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"User {user.FullName} created successfully.";
            return RedirectToAction(nameof(ManageAll));
        }

        // EDIT GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
        var u = await _context.Users.FindAsync(id);
        if (u == null) return NotFound();

        // Fetch existing permissions for this role
        var existingPerms = await _context.RolePermissions
            .Where(rp => rp.RoleId == u.RoleId)
            .Select(rp => rp.ModuleName)
            .ToListAsync();

        var vm = new TeamUserFormViewModel
        {
            UserId       = u.UserId,
            Username     = u.Username,
            Email        = u.Email,
            FullName     = u.FullName ?? "",
            PhoneNumber  = u.PhoneNumber,
            RoleId       = u.RoleId,
            DepartmentId = u.DepartmentId,
            IsActive     = u.IsActive,
            SelectedPermissions = existingPerms
        };

        ViewBag.Roles = await _context.Roles.ToListAsync();
        ViewBag.Departments = await _context.Departments.ToListAsync();
        return View(vm);
        }

       // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TeamUserFormViewModel vm)
        {
            if (id != vm.UserId) return BadRequest();
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.Roles.ToListAsync();
                ViewBag.Departments = await _context.Departments.ToListAsync();
                return View(vm);
            }

            var u = await _context.Users.FindAsync(id);
            if (u == null) return NotFound();

            u.FullName     = vm.FullName;
            u.PhoneNumber  = vm.PhoneNumber;
            u.RoleId       = vm.RoleId;
            u.DepartmentId = vm.DepartmentId;
            u.IsActive     = vm.IsActive;
            u.UpdatedAt    = DateTime.Now;

            if (!string.IsNullOrEmpty(vm.Password))
                u.Password = vm.Password;

            // ═══ UPDATE PERMISSIONS ═══
            // 1. Purani permissions delete karo us role ki
            var oldPerms = await _context.RolePermissions.Where(rp => rp.RoleId == vm.RoleId).ToListAsync();
            _context.RolePermissions.RemoveRange(oldPerms);

            // 2. Nayi permissions add karo
            if (vm.SelectedPermissions != null && vm.SelectedPermissions.Any())
            {
                foreach (var module in vm.SelectedPermissions)
                {
                    _context.RolePermissions.Add(new RolePermission 
                    { 
                        RoleId = vm.RoleId, 
                        ModuleName = module 
                    });
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"User {u.FullName} updated.";
            return RedirectToAction(nameof(ManageAll));
        }

        // TOGGLE ACTIVE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var u = await _context.Users.FindAsync(id);
            if (u == null) return NotFound();

            // Prevent admin from deactivating self
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(currentUserId, out var myId) && myId == id)
            {
                TempData["Error"] = "Cannot deactivate yourself.";
                return RedirectToAction(nameof(ManageAll));
            }

            u.IsActive = !u.IsActive;
            u.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{(u.IsActive ? "Activated" : "Deactivated")} {u.FullName}.";
            return RedirectToAction(nameof(ManageAll));
        }

        // ═══════════ DELETE USER (sirf inactive) ═══════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var u = await _context.Users.FindAsync(id);
            if (u == null) return NotFound();

            // Security check: cannot delete self
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(currentUserId, out var myId) && myId == id)
            {
                TempData["Error"] = "You cannot delete yourself.";
                return RedirectToAction(nameof(ManageAll));
            }

            // Safety check: only inactive users can be deleted
            if (u.IsActive)
            {
                TempData["Error"] = $"Cannot delete {u.FullName} — pehle deactivate karo.";
                return RedirectToAction(nameof(ManageAll));
            }

            var name = u.FullName ?? u.Username;
            _context.Users.Remove(u);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"User {name} deleted.";
            return RedirectToAction(nameof(ManageAll));
        }


    }
}