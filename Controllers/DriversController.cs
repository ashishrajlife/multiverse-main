using ERPDemo.Data;
using ERPDemo.Models;
using ERPDemo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPDemo.Controllers
{
    [Authorize]
    public class DriversController : Controller
    {
        private readonly AppDbContext _context;

        public DriversController(AppDbContext context)
        {
            _context = context;
        }

        // ═══════════ LIST ═══════════
        public async Task<IActionResult> Index(string? search, string? status)
        {
            var query = _context.Drivers
                .Include(d => d.Vehicle)
                .Include(d => d.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    d.FullName.Contains(search) ||
                    (d.PhoneNumber != null && d.PhoneNumber.Contains(search)) ||
                    (d.LicenseNumber != null && d.LicenseNumber.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(d => d.Status == status);
            }

            var drivers = await query
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status ?? "All";
            ViewBag.StatusOptions = new List<string> { "All", "Available", "OnTrip", "OffDuty", "Leave" };

            return View(drivers);
        }

        // ═══════════ DETAILS ═══════════
        public async Task<IActionResult> Details(int id)
        {
            var driver = await _context.Drivers
                .Include(d => d.Vehicle)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DriverId == id);

            if (driver == null) return NotFound();

            ViewBag.RecentTrips = await _context.Trips
                .Include(t => t.Vehicle)
                .Where(t => t.DriverId == id)
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .ToListAsync();

            // GPS-based stats (sum from completed trips)
            var completedTrips = await _context.Trips
                .Where(t => t.DriverId == id && t.Status == "Completed")
                .ToListAsync();

            ViewBag.TotalTrips      = completedTrips.Count;
            ViewBag.TotalDistanceKm = completedTrips.Sum(t => (decimal?)t.GpsDistanceKm) ?? 0;
            ViewBag.TotalRevenue    = completedTrips.Sum(t => t.Revenue ?? 0);
            ViewBag.TotalIdleMin    = completedTrips.Sum(t => t.GpsIdleMinutes ?? 0);
            ViewBag.MaxSpeedKmph    = completedTrips.Max(t => (int?)t.GpsMaxSpeedKmph) ?? 0;

            return View(driver);
        }

        // ═══════════ CREATE GET ═══════════
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new DriverFormViewModel();
            await PopulateDropdowns(vm);
            return View(vm);
        }

        // ═══════════ CREATE POST ═══════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DriverFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(vm);
                return View(vm);
            }

            var exists = await _context.Drivers
                .AnyAsync(d => d.LicenseNumber == vm.LicenseNumber);
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.LicenseNumber), "Ye license number already registered hai.");
                await PopulateDropdowns(vm);
                return View(vm);
            }

            var driver = new Driver
            {
                FullName       = vm.FullName,
                PhoneNumber    = vm.PhoneNumber,
                LicenseNumber  = vm.LicenseNumber.ToUpper().Trim(),
                LicenseExpiry  = vm.LicenseExpiry,
                Status         = vm.Status,
                VehicleId      = vm.VehicleId,
                UserId         = vm.UserId,
                DepotLocation  = vm.DepotLocation,
                CreatedAt      = DateTime.Now
            };

            _context.Drivers.Add(driver);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Driver {driver.FullName} added.";
            return RedirectToAction(nameof(Index));
        }

        // ═══════════ EDIT GET ═══════════
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var d = await _context.Drivers.FindAsync(id);
            if (d == null) return NotFound();

            var vm = new DriverFormViewModel
            {
                DriverId      = d.DriverId,
                FullName      = d.FullName,
                PhoneNumber   = d.PhoneNumber ?? "",
                LicenseNumber = d.LicenseNumber ?? "",
                LicenseExpiry = d.LicenseExpiry,
                Status        = d.Status,
                VehicleId     = d.VehicleId,
                UserId        = d.UserId,
                DepotLocation = d.DepotLocation
            };

            await PopulateDropdowns(vm);
            return View(vm);
        }

        // ═══════════ EDIT POST ═══════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DriverFormViewModel vm)
        {
            if (id != vm.DriverId) return BadRequest();
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(vm);
                return View(vm);
            }

            var d = await _context.Drivers.FindAsync(id);
            if (d == null) return NotFound();

            var exists = await _context.Drivers
                .AnyAsync(x => x.LicenseNumber == vm.LicenseNumber && x.DriverId != id);
            if (exists)
            {
                ModelState.AddModelError(nameof(vm.LicenseNumber), "Ye license number kisi aur driver ka hai.");
                await PopulateDropdowns(vm);
                return View(vm);
            }

            d.FullName      = vm.FullName;
            d.PhoneNumber   = vm.PhoneNumber;
            d.LicenseNumber = vm.LicenseNumber.ToUpper().Trim();
            d.LicenseExpiry = vm.LicenseExpiry;
            d.Status        = vm.Status;
            d.VehicleId     = vm.VehicleId;
            d.UserId        = vm.UserId;
            d.DepotLocation = vm.DepotLocation;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Driver {d.FullName} updated.";
            return RedirectToAction(nameof(Index));
        }

        // ═══════════ TOGGLE STATUS ═══════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var driver = await _context.Drivers.FindAsync(id);
            if (driver == null) return NotFound();

            if (driver.Status == "OnTrip")
            {
                TempData["Error"] = $"{driver.FullName} currently on trip hai — pehle trip complete karo.";
                return RedirectToAction(nameof(Index));
            }
            if (driver.Status == "Leave")
            {
                TempData["Error"] = $"{driver.FullName} leave pe hai — Edit se status change karo.";
                return RedirectToAction(nameof(Index));
            }

            if (driver.Status == "Available")
            {
                driver.Status = "OffDuty";
                TempData["Success"] = $"{driver.FullName} off duty.";
            }
            else
            {
                driver.Status = "Available";
                TempData["Success"] = $"{driver.FullName} available.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ═══════════ DELETE ═══════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var d = await _context.Drivers.FindAsync(id);
            if (d == null) return NotFound();

            var hasTrips = await _context.Trips.AnyAsync(t => t.DriverId == id);
            if (hasTrips)
            {
                TempData["Error"] = $"Cannot delete {d.FullName} — trips history hai.";
                return RedirectToAction(nameof(Index));
            }

            _context.Drivers.Remove(d);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Driver {d.FullName} deleted.";
            return RedirectToAction(nameof(Index));
        }

        // ═══════════ HELPER ═══════════
        private async Task PopulateDropdowns(DriverFormViewModel vm)
        {
            // Available vehicles — Active + Idle
            vm.AvailableVehicles = await _context.Vehicles
                .Where(v => v.Status == "Active" || v.Status == "Idle")
                .OrderBy(v => v.RegistrationNumber)
                .Select(v => new VehicleOption
                {
                    VehicleId = v.VehicleId,
                    Label = v.RegistrationNumber + " — " + v.VehicleType
                })
                .ToListAsync();

            // Users with role "User" not already linked to another driver
            var linkedUserIds = await _context.Drivers
                .Where(d => d.UserId != null && d.DriverId != vm.DriverId)
                .Select(d => d.UserId!.Value)
                .ToListAsync();

            vm.AvailableUsers = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Role != null && u.Role.RoleName == "User"
                         && !linkedUserIds.Contains(u.UserId))
                .OrderBy(u => u.FullName)
                .Select(u => new UserOption
                {
                    UserId = u.UserId,
                    Label = (u.FullName ?? u.Username) + " (" + u.Username + ")"
                })
                .ToListAsync();
        }
    }
}