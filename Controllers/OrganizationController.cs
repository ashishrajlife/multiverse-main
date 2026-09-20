using ERPDemo.Data;
using ERPDemo.Filters;
using ERPDemo.Models;
using ERPDemo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPDemo.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    [NoCache]
    public class OrganizationController : Controller
    {
        private readonly AppDbContext _context;

        public OrganizationController(AppDbContext context)
        {
            _context = context;
        }

        // ---------- LIST ----------
        public async Task<IActionResult> Index(string? search, string? status)
        {
            var query = _context.Organizations
                .Include(o => o.Users)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(o =>
                    o.Name.ToLower().Contains(s) ||
                    o.Code.ToLower().Contains(s) ||
                    (o.Email != null && o.Email.ToLower().Contains(s)));
            }

            if (status == "active") query = query.Where(o => o.IsActive);
            else if (status == "inactive") query = query.Where(o => !o.IsActive);

            var rows = await query
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrganizationRow
                {
                    OrganizationId = o.OrganizationId,
                    Name = o.Name,
                    Code = o.Code,
                    Email = o.Email,
                    Phone = o.Phone,
                    City = o.City,
                    Country = o.Country,
                    Plan = o.Plan,
                    IsActive = o.IsActive,
                    UserCount = o.Users.Count,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            var vm = new OrganizationViewModel
            {
                Organizations = rows,
                SearchTerm = search,
                StatusFilter = status,
                TotalOrganizations = rows.Count,
                ActiveOrganizations = rows.Count(o => o.IsActive),
                TotalUsersInOrgs = rows.Sum(o => o.UserCount)
            };

            return View(vm);
        }

        // ---------- CREATE ----------
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateOrganizationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrganizationViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _context.Organizations.AnyAsync(o => o.Code == model.Code))
            {
                ModelState.AddModelError("Code", "This code is already in use.");
                return View(model);
            }
            if (!string.IsNullOrWhiteSpace(model.Email) &&
                await _context.Organizations.AnyAsync(o => o.Email == model.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            _context.Organizations.Add(new Organization
            {
                Name = model.Name,
                Code = model.Code.ToUpper(),
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                City = model.City,
                Country = model.Country,
                Plan = model.Plan,
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Organization '{model.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ---------- EDIT ----------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var org = await _context.Organizations.FindAsync(id);
            if (org == null) return NotFound();

            var vm = new EditOrganizationViewModel
            {
                OrganizationId = org.OrganizationId,
                Name = org.Name,
                Code = org.Code,
                Email = org.Email,
                Phone = org.Phone,
                Address = org.Address,
                City = org.City,
                Country = org.Country,
                Plan = org.Plan,
                IsActive = org.IsActive
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditOrganizationViewModel model)
        {
            var org = await _context.Organizations.FindAsync(model.OrganizationId);
            if (org == null) return NotFound();
            if (!ModelState.IsValid) return View(model);

            if (await _context.Organizations.AnyAsync(o => o.Code == model.Code && o.OrganizationId != model.OrganizationId))
            {
                ModelState.AddModelError("Code", "This code is already in use.");
                return View(model);
            }
            if (!string.IsNullOrWhiteSpace(model.Email) &&
                await _context.Organizations.AnyAsync(o => o.Email == model.Email && o.OrganizationId != model.OrganizationId))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            org.Name = model.Name;
            org.Code = model.Code.ToUpper();
            org.Email = model.Email;
            org.Phone = model.Phone;
            org.Address = model.Address;
            org.City = model.City;
            org.Country = model.Country;
            org.Plan = model.Plan;
            org.IsActive = model.IsActive;
            org.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Organization updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ---------- TOGGLE ACTIVE ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var org = await _context.Organizations.FindAsync(id);
            if (org == null) return NotFound();

            org.IsActive = !org.IsActive;
            org.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{org.Name}' {(org.IsActive ? "activated" : "deactivated")}.";
            return RedirectToAction(nameof(Index));
        }

        // ---------- DETAILS ----------
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var org = await _context.Organizations
                .Include(o => o.Users).ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(o => o.OrganizationId == id);

            if (org == null) return NotFound();
            return View(org);
        }

       // ---------- DELETE ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var org = await _context.Organizations.FindAsync(id);
            if (org == null) return NotFound();

            _context.Organizations.Remove(org);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{org.Name}' deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}