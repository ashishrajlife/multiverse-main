using ERPDemo.Data;
using ERPDemo.Models;
using ERPDemo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPDemo.Controllers
{
    [Authorize]
    public class VehiclesController : Controller
    {
        private readonly AppDbContext _context;

        public VehiclesController(AppDbContext context)
        {
            _context = context;
        }

        // ═══════════ LIST ═══════════
        public async Task<IActionResult> Index(string? search, string? status)
        {
            var query = _context.Vehicles.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(v =>
                    v.RegistrationNumber.Contains(search) ||
                    (v.Make != null && v.Make.Contains(search)) ||
                    (v.Model != null && v.Model.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(v => v.Status == status);
            }

            var vehicles = await query
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status ?? "All";
            ViewBag.StatusOptions = new List<string> { "All", "Active", "Idle", "Maintenance", "Retired" };

            return View(vehicles);
        }

        // ═══════════ DETAILS ═══════════
        public async Task<IActionResult> Details(int id)
        {
          var vehicle = await _context.Vehicles
         .FirstOrDefaultAsync(v => v.VehicleId == id);

            if (vehicle == null) return NotFound();

            // Recent trips of this vehicle
            ViewBag.RecentTrips = await _context.Trips
                .Include(t => t.Driver)
                .Where(t => t.VehicleId == id)
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .ToListAsync();

            ViewBag.MaintenanceLogs = await _context.MaintenanceLogs
                .Where(m => m.VehicleId == id)
                .OrderByDescending(m => m.ServiceDate)
                .Take(10)
                .ToListAsync();

            ViewBag.FuelLogs = await _context.FuelLogs
                .Where(f => f.VehicleId == id)
                .OrderByDescending(f => f.FilledAt)
                .Take(10)
                .ToListAsync();

            return View(vehicle);
        }

        // ═══════════ CREATE GET ═══════════
        [HttpGet]
        public IActionResult Create()
        {
            return View(new VehicleFormViewModel());
        }

        // ═══════════ CREATE POST ═══════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Duplicate reg number check
            var exists = await _context.Vehicles
                .AnyAsync(v => v.RegistrationNumber == vm.RegistrationNumber);
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.RegistrationNumber), "Ye registration number already registered hai.");
                return View(vm);
            }

            var vehicle = new Vehicle
            {
                RegistrationNumber = vm.RegistrationNumber.ToUpper().Trim(),
                VehicleType        = vm.VehicleType,
                Make               = vm.Make,
                Model              = vm.Model,
                Year               = vm.Year,
                CapacityTons       = vm.CapacityTons,
                Status             = vm.Status,
                FuelType           = vm.FuelType,
                CurrentOdometerKm  = vm.CurrentOdometerKm,
                InsuranceExpiry    = vm.InsuranceExpiry,
                FitnessExpiry      = vm.FitnessExpiry,
                PermitExpiry       = vm.PermitExpiry,
                PucExpiry          = vm.PucExpiry,
                LastServiceDate    = vm.LastServiceDate,
                NextServiceDue     = vm.NextServiceDue,
                PlantLocation      = vm.PlantLocation,
                CreatedAt          = DateTime.Now
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Vehicle {vehicle.RegistrationNumber} added successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ═══════════ EDIT GET ═══════════
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var v = await _context.Vehicles.FindAsync(id);
            if (v == null) return NotFound();

            var vm = new VehicleFormViewModel
            {
                VehicleId          = v.VehicleId,
                RegistrationNumber = v.RegistrationNumber,
                VehicleType        = v.VehicleType,
                Make               = v.Make,
                Model              = v.Model,
                Year               = v.Year,
                CapacityTons       = v.CapacityTons,
                Status             = v.Status,
                FuelType           = v.FuelType,
                CurrentOdometerKm  = v.CurrentOdometerKm,
                InsuranceExpiry    = v.InsuranceExpiry,
                FitnessExpiry      = v.FitnessExpiry,
                PermitExpiry       = v.PermitExpiry,
                PucExpiry          = v.PucExpiry,
                LastServiceDate    = v.LastServiceDate,
                NextServiceDue     = v.NextServiceDue,
                PlantLocation      = v.PlantLocation
            };
            return View(vm);
        }

        // ═══════════ EDIT POST ═══════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VehicleFormViewModel vm)
        {
            if (id != vm.VehicleId) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var v = await _context.Vehicles.FindAsync(id);
            if (v == null) return NotFound();

            // Duplicate check (ignore self)
            var exists = await _context.Vehicles
                .AnyAsync(x => x.RegistrationNumber == vm.RegistrationNumber && x.VehicleId != id);
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.RegistrationNumber), "Ye registration number kisi aur vehicle ka hai.");
                return View(vm);
            }

            v.RegistrationNumber = vm.RegistrationNumber.ToUpper().Trim();
            v.VehicleType        = vm.VehicleType;
            v.Make               = vm.Make;
            v.Model              = vm.Model;
            v.Year               = vm.Year;
            v.CapacityTons       = vm.CapacityTons;
            v.Status             = vm.Status;
            v.FuelType           = vm.FuelType;
            v.CurrentOdometerKm  = vm.CurrentOdometerKm;
            v.InsuranceExpiry    = vm.InsuranceExpiry;
            v.FitnessExpiry      = vm.FitnessExpiry;
            v.PermitExpiry       = vm.PermitExpiry;
            v.PucExpiry          = vm.PucExpiry;
            v.LastServiceDate    = vm.LastServiceDate;
            v.NextServiceDue     = vm.NextServiceDue;
            v.PlantLocation      = vm.PlantLocation;
            v.UpdatedAt          = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Vehicle {v.RegistrationNumber} updated.";
            return RedirectToAction(nameof(Index));
        }

        // ═══════════ TOGGLE ACTIVE / INACTIVE ═══════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();

            // Retired vehicle ko wapas active nahi kar sakte — pehle edit se status change karo
            if (vehicle.Status == "Retired")
            {
                TempData["Error"] = $"{vehicle.RegistrationNumber} retired hai — Edit se status change karo.";
                return RedirectToAction(nameof(Index));
            }

            // Active <-> Idle toggle
            if (vehicle.Status == "Active")
            {
                vehicle.Status = "Idle";
                TempData["Success"] = $"{vehicle.RegistrationNumber} deactivated (Idle).";
            }
            else
            {
                vehicle.Status = "Active";
                TempData["Success"] = $"{vehicle.RegistrationNumber} activated.";
            }

            vehicle.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ═══════════ DELETE ═══════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var v = await _context.Vehicles.FindAsync(id);
            if (v == null) return NotFound();

            // Prevent delete if vehicle has trips
            var hasTrips = await _context.Trips.AnyAsync(t => t.VehicleId == id);
            if (hasTrips)
            {
                TempData["Error"] = $"Cannot delete {v.RegistrationNumber} — it has trip history.";
                return RedirectToAction(nameof(Index));
            }

            _context.Vehicles.Remove(v);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Vehicle {v.RegistrationNumber} deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}