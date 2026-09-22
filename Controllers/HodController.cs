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
    [Authorize(Roles = "HOD")]
    [NoCache]
    public class HodController : Controller
    {
        private readonly AppDbContext _context;

        public HodController(AppDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        private async Task<User?> GetCurrentHodAsync()
        {
            return await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
        }

        // ============ DASHBOARD ============
        public async Task<IActionResult> Dashboard()
        {
            var hod = await GetCurrentHodAsync();
            if (hod == null) return RedirectToAction("Login", "Auth");

            if (hod.DepartmentId == null)
            {
                TempData["Error"] = "Aapko koi department assign nahi hai. Admin se sampark karein.";
                return RedirectToAction("AdminDashboard", "Dashboard");
            }

            // HOD ke department ke saare Managers aur Employees
            var teamMembers = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.OrganizationId == hod.OrganizationId
                         && u.DepartmentId == hod.DepartmentId
                         && (u.RoleId == 1 || u.RoleId == 2))   // Employee + Manager
                .OrderBy(u => u.RoleId).ThenBy(u => u.FullName)
                .ToListAsync();

            ViewBag.HodName = hod.FullName ?? hod.Username;
            ViewBag.DepartmentName = hod.Department?.Name ?? "Unassigned";
            ViewBag.TotalCount = teamMembers.Count;
            ViewBag.ManagerCount = teamMembers.Count(m => m.RoleId == 2);
            ViewBag.EmployeeCount = teamMembers.Count(m => m.RoleId == 1);
            ViewBag.ActiveCount = teamMembers.Count(m => m.IsActive);
            ViewBag.TeamMembers = teamMembers;

            return View();
        }

        // ============ CREATE (Add Member) ============
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var hod = await GetCurrentHodAsync();
            if (hod == null) return RedirectToAction("Login", "Auth");
            if (hod.DepartmentId == null)
            {
                TempData["Error"] = "Aapko koi department assign nahi hai.";
                return RedirectToAction(nameof(Dashboard));
            }

            // HOD sirf Employee (1) aur Manager (2) bana sakta hai
            ViewBag.Roles = await _context.Roles
                .Where(r => r.RoleId == 1 || r.RoleId == 2)
                .OrderBy(r => r.RoleId)
                .ToListAsync();

            ViewBag.Departments = await _context.Departments
                .Where(d => d.DepartmentId == hod.DepartmentId)
                .ToListAsync();

            ViewBag.LockedDepartmentName = hod.Department?.Name;

            var vm = new CreateTeamMemberViewModel
            {
                DepartmentId = hod.DepartmentId
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTeamMemberViewModel model)
        {
            var hod = await GetCurrentHodAsync();
            if (hod == null) return RedirectToAction("Login", "Auth");
            if (hod.DepartmentId == null)
            {
                TempData["Error"] = "Aapko koi department assign nahi hai.";
                return RedirectToAction(nameof(Dashboard));
            }

            // 🔒 Security: HOD sirf apne department mein add kare
            model.DepartmentId = hod.DepartmentId;

            // 🔒 HOD sirf Employee ya Manager assign kare
            if (model.RoleId != 1 && model.RoleId != 2)
            {
                ModelState.AddModelError("RoleId", "Aap sirf Employee ya Manager assign kar sakte hai.");
            }

            ViewBag.Roles = await _context.Roles
                .Where(r => r.RoleId == 1 || r.RoleId == 2)
                .OrderBy(r => r.RoleId)
                .ToListAsync();
            ViewBag.Departments = await _context.Departments
                .Where(d => d.DepartmentId == hod.DepartmentId).ToListAsync();
            ViewBag.LockedDepartmentName = hod.Department?.Name;

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
                Password = model.RoleId == 1 
                            ? Guid.NewGuid().ToString()   // Employee — dummy password
                            : model.Password,             // Manager — real password
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                RoleId = model.RoleId,
                DepartmentId = hod.DepartmentId,
                OrganizationId = hod.OrganizationId,
                IsActive = true,
                CreatedAt = DateTime.Now,

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
            TempData["Success"] = $"'{model.Username}' add ho gaya.";
            return RedirectToAction(nameof(Dashboard));
        }

        // ============ EDIT ============
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var hod = await GetCurrentHodAsync();
            if (hod == null) return RedirectToAction("Login", "Auth");

            // 🔒 HOD sirf apne dept ka Employee/Manager edit kare
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id
                                       && u.OrganizationId == hod.OrganizationId
                                       && u.DepartmentId == hod.DepartmentId
                                       && (u.RoleId == 1 || u.RoleId == 2));

            if (user == null)
            {
                TempData["Error"] = "User nahi mila ya aap is department ke user nahi ho.";
                return RedirectToAction(nameof(Dashboard));
            }

            ViewBag.Roles = await _context.Roles
                .Where(r => r.RoleId == 1 || r.RoleId == 2)
                .OrderBy(r => r.RoleId)
                .ToListAsync();
            ViewBag.Departments = await _context.Departments
                .Where(d => d.DepartmentId == hod.DepartmentId).ToListAsync();
            ViewBag.LockedDepartmentName = hod.Department?.Name;

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
            var hod = await GetCurrentHodAsync();
            if (hod == null) return RedirectToAction("Login", "Auth");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == model.UserId
                                       && u.OrganizationId == hod.OrganizationId
                                       && u.DepartmentId == hod.DepartmentId
                                       && (u.RoleId == 1 || u.RoleId == 2));

            if (user == null)
            {
                TempData["Error"] = "User nahi mila ya aap is department ke user nahi ho.";
                return RedirectToAction(nameof(Dashboard));
            }

            // 🔒 Force department
            model.DepartmentId = hod.DepartmentId;

            if (model.RoleId != 1 && model.RoleId != 2)
            {
                ModelState.AddModelError("RoleId", "Aap sirf Employee ya Manager assign kar sakte hai.");
            }

            ViewBag.Roles = await _context.Roles
                .Where(r => r.RoleId == 1 || r.RoleId == 2)
                .OrderBy(r => r.RoleId)
                .ToListAsync();
            ViewBag.Departments = await _context.Departments
                .Where(d => d.DepartmentId == hod.DepartmentId).ToListAsync();
            ViewBag.LockedDepartmentName = hod.Department?.Name;

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
            user.RoleId = model.RoleId;
            user.IsActive = model.IsActive;

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
            var hod = await GetCurrentHodAsync();
            if (hod == null) return RedirectToAction("Login", "Auth");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id
                                       && u.OrganizationId == hod.OrganizationId
                                       && u.DepartmentId == hod.DepartmentId
                                       && (u.RoleId == 1 || u.RoleId == 2));

            if (user == null)
            {
                TempData["Error"] = "User nahi mila.";
                return RedirectToAction(nameof(Dashboard));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{user.Username}' delete ho gaya.";
            return RedirectToAction(nameof(Dashboard));
        }

        // ============ RESET PASSWORD ============
[HttpGet]
public async Task<IActionResult> ResetPassword(int id)
{
    var hod = await GetCurrentHodAsync();
    if (hod == null) return RedirectToAction("Login", "Auth");

    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.UserId == id
                               && u.OrganizationId == hod.OrganizationId
                               && u.DepartmentId == hod.DepartmentId
                               && (u.RoleId == 1 || u.RoleId == 2));

    if (user == null)
    {
        TempData["Error"] = "User nahi mila.";
        return RedirectToAction(nameof(Dashboard));
    }

    // 🔒 HOD khud ka password yahan se reset nahi kar sakta
    if (user.UserId == CurrentUserId)
    {
        TempData["Error"] = "Aap apna password yahan se reset nahi kar sakte.";
        return RedirectToAction(nameof(Dashboard));
    }

    ViewBag.UserName = user.FullName ?? user.Username;
    return View(new ResetPasswordViewModel { UserId = user.UserId });
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
{
    var hod = await GetCurrentHodAsync();
    if (hod == null) return RedirectToAction("Login", "Auth");

    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.UserId == model.UserId
                               && u.OrganizationId == hod.OrganizationId
                               && u.DepartmentId == hod.DepartmentId
                               && (u.RoleId == 1 || u.RoleId == 2));

    if (user == null)
    {
        TempData["Error"] = "User nahi mila.";
        return RedirectToAction(nameof(Dashboard));
    }

    // 🔒 Khud ka password reset nahi
    if (user.UserId == CurrentUserId)
    {
        TempData["Error"] = "Aap apna password yahan se reset nahi kar sakte.";
        return RedirectToAction(nameof(Dashboard));
    }

    if (!ModelState.IsValid)
    {
        ViewBag.UserName = user.FullName ?? user.Username;
        return View(model);
    }

    user.Password = model.NewPassword!;
    user.UpdatedAt = DateTime.Now;
    await _context.SaveChangesAsync();

    TempData["Success"] = $"'{user.Username}' ka password reset ho gaya.";
    return RedirectToAction(nameof(Dashboard));
}
      
        
    }
}