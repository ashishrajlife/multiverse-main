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

        // Helper: logged-in manager ko fetch karo
        private async Task<User?> GetCurrentManagerAsync()
        {
            return await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
        }

        // ============ DASHBOARD ============
        public async Task<IActionResult> Dashboard()
        {
            var manager = await GetCurrentManagerAsync();
            if (manager == null) return RedirectToAction("Login", "Auth");

            var teamMembers = await _context.Users
                .Where(u => u.OrganizationId == manager.OrganizationId
                         && u.DepartmentId == manager.DepartmentId
                         && u.RoleId == 1)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.ManagerName = manager.FullName ?? manager.Username;
            ViewBag.DepartmentName = manager.Department?.Name ?? "Unassigned";
            ViewBag.TeamCount = teamMembers.Count;
            ViewBag.ActiveCount = teamMembers.Count(m => m.IsActive);
            ViewBag.InactiveCount = teamMembers.Count(m => !m.IsActive);
            ViewBag.TeamMembers = teamMembers;

            return View();
        }

        // ============ CREATE (ADD MEMBER) ============
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var manager = await GetCurrentManagerAsync();
            if (manager == null) return RedirectToAction("Login", "Auth");
            if (manager.DepartmentId == null)
            {
                TempData["Error"] = "Aapko koi department assign nahi hai. Admin se sampark karein.";
                return RedirectToAction(nameof(Dashboard));
            }

            // Manager sirf Employee role assign kar sakta hai
            ViewBag.Roles = await _context.Roles
                .Where(r => r.RoleId == 1)
                .ToListAsync();

            ViewBag.Departments = await _context.Departments
                .Where(d => d.DepartmentId == manager.DepartmentId)
                .ToListAsync();

            var vm = new CreateTeamMemberViewModel
            {
                DepartmentId = manager.DepartmentId   // 👈 Pre-select
            };

            ViewBag.LockedDepartmentName = manager.Department?.Name;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTeamMemberViewModel model)
        {
            var manager = await GetCurrentManagerAsync();
            if (manager == null) return RedirectToAction("Login", "Auth");
            if (manager.DepartmentId == null)
            {
                TempData["Error"] = "Aapko koi department assign nahi hai.";
                return RedirectToAction(nameof(Dashboard));
            }

            // 🔒 Security: Manager sirf apne department mein hi add kar sakta hai
            model.DepartmentId = manager.DepartmentId;
            model.RoleId = 1;   // Force Employee role

            ViewBag.Roles = await _context.Roles.Where(r => r.RoleId == 1).ToListAsync();
            ViewBag.Departments = await _context.Departments
                .Where(d => d.DepartmentId == manager.DepartmentId).ToListAsync();
            ViewBag.LockedDepartmentName = manager.Department?.Name;

            if (!ModelState.IsValid) return View(model);

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
                RoleId = 1,                              // Employee
                DepartmentId = manager.DepartmentId,     // Manager ka dept
                OrganizationId = manager.OrganizationId,
                IsActive = true,
                CreatedAt = DateTime.Now,

                // Extra fields
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
                SalaryAmount = model.SalaryAmount
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = $"'{model.Username}' employee add ho gaya.";
            return RedirectToAction(nameof(Dashboard));
        }

        // ============ EDIT ============
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var manager = await GetCurrentManagerAsync();
            if (manager == null) return RedirectToAction("Login", "Auth");

            // 🔒 Security: sirf apne department ka employee edit ho sakta hai
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id
                                       && u.OrganizationId == manager.OrganizationId
                                       && u.DepartmentId == manager.DepartmentId
                                       && u.RoleId == 1);

            if (user == null)
            {
                TempData["Error"] = "Employee nahi mila ya aap is department ke employee nahi ho.";
                return RedirectToAction(nameof(Dashboard));
            }

            ViewBag.Roles = await _context.Roles.Where(r => r.RoleId == 1).ToListAsync();
            ViewBag.Departments = await _context.Departments
                .Where(d => d.DepartmentId == manager.DepartmentId).ToListAsync();
            ViewBag.LockedDepartmentName = manager.Department?.Name;

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
                City = user.City,
                State = user.State,
                Pincode = user.Pincode,
                AadhaarNumber = user.AadhaarNumber,
                PanNumber = user.PanNumber,
                EmploymentType = user.EmploymentType,
                Designation = user.Designation,
                PlantLocation = user.PlantLocation,
                JoiningDate = user.JoiningDate,
                ShiftStartTime = user.ShiftStartTime,
                ShiftEndTime = user.ShiftEndTime,
                SalaryType = user.SalaryType,
                SalaryAmount = user.SalaryAmount
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditTeamMemberViewModel model)
        {
            var manager = await GetCurrentManagerAsync();
            if (manager == null) return RedirectToAction("Login", "Auth");

            // 🔒 Security: sirf apne department ka employee edit
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == model.UserId
                                       && u.OrganizationId == manager.OrganizationId
                                       && u.DepartmentId == manager.DepartmentId
                                       && u.RoleId == 1);

            if (user == null)
            {
                TempData["Error"] = "Employee nahi mila ya aap is department ke employee nahi ho.";
                return RedirectToAction(nameof(Dashboard));
            }

            // 🔒 Force department and role
            model.DepartmentId = manager.DepartmentId;
            model.RoleId = 1;

            ViewBag.Roles = await _context.Roles.Where(r => r.RoleId == 1).ToListAsync();
            ViewBag.Departments = await _context.Departments
                .Where(d => d.DepartmentId == manager.DepartmentId).ToListAsync();
            ViewBag.LockedDepartmentName = manager.Department?.Name;

            if (!ModelState.IsValid) return View(model);

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
            user.IsActive = model.IsActive;

            // Extra fields
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

            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Employee details update ho gayi.";
            return RedirectToAction(nameof(Dashboard));
        }

        // ============ DELETE ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var manager = await GetCurrentManagerAsync();
            if (manager == null) return RedirectToAction("Login", "Auth");

            // 🔒 Security: sirf apne department ka employee delete
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id
                                       && u.OrganizationId == manager.OrganizationId
                                       && u.DepartmentId == manager.DepartmentId
                                       && u.RoleId == 1);

            if (user == null)
            {
                TempData["Error"] = "Employee nahi mila ya aap is department ke employee nahi ho.";
                return RedirectToAction(nameof(Dashboard));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{user.Username}' delete ho gaya.";
            return RedirectToAction(nameof(Dashboard));
        }
    }
}