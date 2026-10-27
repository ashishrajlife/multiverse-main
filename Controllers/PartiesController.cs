using ERPDemo.Data;
using ERPDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPDemo.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class PartiesController : Controller
    {
        private readonly AppDbContext _context;

        public PartiesController(AppDbContext context)
        {
            _context = context;
        }

        // LIST
        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Parties.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => 
                    p.PartyName.Contains(search) || 
                    (p.PhoneNumber != null && p.PhoneNumber.Contains(search)) ||
                    (p.Gstin != null && p.Gstin.Contains(search)));
            }

            var parties = await query.OrderBy(p => p.PartyName).ToListAsync();
            ViewBag.Search = search;
            return View(parties);
        }

        // CREATE GET
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Party());
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Party model)
        {
            if (!ModelState.IsValid) return View(model);

            _context.Parties.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Party {model.PartyName} added.";
            return RedirectToAction(nameof(Index));
        }

        // EDIT GET
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var party = await _context.Parties.FindAsync(id);
            if (party == null) return NotFound();
            return View(party);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Party model)
        {
            if (id != model.PartyId) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            _context.Parties.Update(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Party {model.PartyName} updated.";
            return RedirectToAction(nameof(Index));
        }

        // TOGGLE ACTIVE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var party = await _context.Parties.FindAsync(id);
            if (party == null) return NotFound();

            party.IsActive = !party.IsActive;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{party.PartyName} {(party.IsActive ? "activated" : "deactivated")}.";
            return RedirectToAction(nameof(Index));
        }
    }
}