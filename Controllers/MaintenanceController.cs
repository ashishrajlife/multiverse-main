using ERPDemo.Data;
using ERPDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPDemo.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class MaintenanceController : Controller
    {
        private readonly AppDbContext _context;

        public MaintenanceController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? status)
        {
            var query = _context.MaintenanceLogs
                .Include(m => m.Vehicle)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.Vehicle != null && m.Vehicle.RegistrationNumber.Contains(search));

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
                query = query.Where(m => m.Status == status);

            var logs = await query.OrderByDescending(m => m.ServiceDate).ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status ?? "All";
            ViewBag.StatusOptions = new List<string> { "All", "Scheduled", "InProgress", "Completed" };
            ViewBag.TotalCost = logs.Sum(m => m.Cost ?? 0);

            return View(logs);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Vehicles = await _context.Vehicles.OrderBy(v => v.RegistrationNumber).ToListAsync();
            return View(new MaintenanceLog { ServiceDate = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MaintenanceLog model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Vehicles = await _context.Vehicles.OrderBy(v => v.RegistrationNumber).ToListAsync();
                return View(model);
            }

            _context.MaintenanceLogs.Add(model);

            // If InProgress → set vehicle Maintenance status
            if (model.Status == "InProgress")
            {
                var vehicle = await _context.Vehicles.FindAsync(model.VehicleId);
                if (vehicle != null) vehicle.Status = "Maintenance";
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Maintenance log added — {model.ServiceType}";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var log = await _context.MaintenanceLogs
                .Include(m => m.Vehicle)
                .FirstOrDefaultAsync(m => m.MaintenanceId == id);

            if (log == null) return NotFound();

            log.Status = "Completed";

            if (log.Vehicle != null && log.Vehicle.Status == "Maintenance")
            {
                log.Vehicle.Status = "Active";
                log.Vehicle.LastServiceDate = log.ServiceDate;
                if (log.Vehicle.NextServiceDue.HasValue == false)
                    log.Vehicle.NextServiceDue = log.ServiceDate.AddMonths(3);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Service completed for {log.Vehicle?.RegistrationNumber}";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var log = await _context.MaintenanceLogs.FindAsync(id);
            if (log == null) return NotFound();

            _context.MaintenanceLogs.Remove(log);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Maintenance log deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}