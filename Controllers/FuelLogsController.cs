using ERPDemo.Data;
using ERPDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPDemo.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class FuelLogsController : Controller
    {
        private readonly AppDbContext _context;

        public FuelLogsController(AppDbContext context)
        {
            _context = context;
        }

        // LIST
        public async Task<IActionResult> Index(string? search, int? vehicleId, DateTime? from, DateTime? to)
        {
            var query = _context.FuelLogs
                .Include(f => f.Vehicle)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(f => f.Vehicle != null && f.Vehicle.RegistrationNumber.Contains(search));

            if (vehicleId.HasValue)
                query = query.Where(f => f.VehicleId == vehicleId.Value);

            if (from.HasValue)
                query = query.Where(f => f.FilledAt >= from.Value);

            if (to.HasValue)
               query = query.Where(f => f.FilledAt <= to.Value.AddDays(1));

            var logs = await query
                .OrderByDescending(f => f.FilledAt)
                .Take(500)   // limit
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.VehicleId = vehicleId;
            ViewBag.From = from;
            ViewBag.To = to;
            ViewBag.Vehicles = await _context.Vehicles.OrderBy(v => v.RegistrationNumber).ToListAsync();
            ViewBag.TotalLiters = logs.Sum(f => f.Liters);
            ViewBag.TotalCost = logs.Sum(f => f.Amount);

            return View(logs);
        }

        // CREATE GET
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Vehicles = await _context.Vehicles
                .Where(v => v.Status == "Active" || v.Status == "Idle")
                .OrderBy(v => v.RegistrationNumber)
                .ToListAsync();
            return View(new FuelLog { FilledAt = DateTime.Now });
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FuelLog model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Vehicles = await _context.Vehicles.OrderBy(v => v.RegistrationNumber).ToListAsync();
                return View(model);
            }

            // Update vehicle odometer if higher
            var vehicle = await _context.Vehicles.FindAsync(model.VehicleId);
            if (vehicle != null && model.OdometerKm.HasValue)
            {
                if (!vehicle.CurrentOdometerKm.HasValue || model.OdometerKm.Value > vehicle.CurrentOdometerKm.Value)
                {
                    vehicle.CurrentOdometerKm = (int)model.OdometerKm.Value;
                }
            }

            _context.FuelLogs.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Fuel log added — {model.Liters:N1} L, ₹{model.Amount:N0}";
            return RedirectToAction(nameof(Index));
        }

        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var log = await _context.FuelLogs.FindAsync(id);
            if (log == null) return NotFound();

            _context.FuelLogs.Remove(log);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Fuel log deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}