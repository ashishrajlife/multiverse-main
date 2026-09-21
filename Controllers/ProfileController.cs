using ERPDemo.Data;
using ERPDemo.Filters;
using ERPDemo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ERPDemo.Controllers
{
    [Authorize]
    [NoCache]
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        // ============ VIEW PROFILE ============
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

            if (user == null) return NotFound();

            var vm = new ProfileViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                RoleName = user.Role?.RoleName ?? "User",
                OrganizationName = user.Organization?.Name,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt
            };

            return View(vm);
        }

        // ============ UPDATE PROFILE ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

            if (user == null) return NotFound();

            // Readonly fields view ke liye refill
            model.RoleName = user.Role?.RoleName ?? "User";
            model.OrganizationName = user.Organization?.Name;
            model.LastLoginAt = user.LastLoginAt;
            model.CreatedAt = user.CreatedAt;

            if (!ModelState.IsValid) return View(model);

            // Email uniqueness check
            if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.UserId != CurrentUserId))
            {
                ModelState.AddModelError("Email", "Ye email kisi aur user ke paas hai.");
                return View(model);
            }

            // Update only editable fields
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Email = model.Email;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // Update session me FullName
            HttpContext.Session.SetString("FullName", model.FullName ?? user.Username);

            TempData["Success"] = "Profile update ho gaya.";
            return RedirectToAction(nameof(Index));
        }

        // ============ CHANGE PASSWORD ============
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users.FindAsync(CurrentUserId);
            if (user == null) return NotFound();

            if (user.Password != model.CurrentPassword)
            {
                ModelState.AddModelError("CurrentPassword", "Purana password galat hai.");
                return View(model);
            }

            user.Password = model.NewPassword;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Password successfully change ho gaya.";
            return RedirectToAction(nameof(ChangePassword));
        }
    }
}