using ERPDemo.Data;
using ERPDemo.Models;
using ERPDemo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPDemo.Controllers
{
    [Authorize]
    public class TripsController : Controller
    {
        private readonly AppDbContext _context;

        public TripsController(AppDbContext context)
        {
            _context = context;
        }

        // ═══════════ LIST ═══════════
        public async Task<IActionResult> Index(string? search, string? status)
        {
            var query = _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Include(t => t.Party)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t =>
                    t.TripNumber.Contains(search) ||
                    (t.LrNumber != null && t.LrNumber.Contains(search)) ||
                    (t.OriginCity != null && t.OriginCity.Contains(search)) ||
                    (t.DestinationCity != null && t.DestinationCity.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(t => t.Status == status);
            }

            var trips = await query
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status ?? "All";
            ViewBag.StatusOptions = new List<string> { "All", "Planned", "InProgress", "Completed", "Delayed", "Cancelled" };

            return View(trips);
        }

        // ═══════════ DETAILS ═══════════
        public async Task<IActionResult> Details(int id)
        {
            var trip = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Include(t => t.Party)
                .FirstOrDefaultAsync(t => t.TripId == id);

            if (trip == null) return NotFound();

            ViewBag.GpsPoints = await _context.GpsLogs
                .Where(g => g.TripId == id)
                .OrderBy(g => g.RecordedAt)
                .Select(g => new { g.Latitude, g.Longitude, g.SpeedKmph, g.RecordedAt })
                .ToListAsync();

            if (trip.VehicleId > 0)
            {
                ViewBag.LatestLocation = await _context.VehicleLocations
                    .FirstOrDefaultAsync(vl => vl.VehicleId == trip.VehicleId);
            }

            return View(trip);
        }

        // ═══════════ CREATE GET ═══════════
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new TripFormViewModel
            {
                PlannedStart = DateTime.Now.AddHours(1),
                PlannedEnd   = DateTime.Now.AddHours(6)
            };
            await PopulateDropdowns(vm);
            return View(vm);
        }

        // ═══════════ CREATE POST ═══════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TripFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(vm);
                return View(vm);
            }

            var vehicleBusy = await _context.Trips.AnyAsync(t =>
                t.VehicleId == vm.VehicleId && (t.Status == "InProgress" || t.Status == "Planned"));
            if (vehicleBusy)
            {
                ModelState.AddModelError(nameof(vm.VehicleId), "Ye vehicle already ek active trip pe hai.");
                await PopulateDropdowns(vm);
                return View(vm);
            }

            var driverBusy = await _context.Trips.AnyAsync(t =>
                t.DriverId == vm.DriverId && (t.Status == "InProgress" || t.Status == "Planned"));
            if (driverBusy)
            {
                ModelState.AddModelError(nameof(vm.DriverId), "Ye driver already ek active trip pe hai.");
                await PopulateDropdowns(vm);
                return View(vm);
            }

            if (vm.PlannedEnd <= vm.PlannedStart)
            {
                ModelState.AddModelError(nameof(vm.PlannedEnd), "Planned end, start se baad hona chahiye.");
                await PopulateDropdowns(vm);
                return View(vm);
            }

            var today = DateTime.Today;
            var todayCount = await _context.Trips
                .CountAsync(t => t.CreatedAt >= today && t.CreatedAt < today.AddDays(1));
            var tripNumber = $"TRP-{today:yyyyMMdd}-{(todayCount + 1):D3}";

            var trip = new Trip
            {
                TripNumber       = tripNumber,
                VehicleId        = vm.VehicleId,
                DriverId         = vm.DriverId,
                OriginCity       = vm.OriginCity,
                DestinationCity  = vm.DestinationCity,
                PlannedStart     = vm.PlannedStart,
                PlannedEnd       = vm.PlannedEnd,
                DistanceKm       = vm.DistanceKm,
                Revenue          = vm.Revenue,
                FuelCost         = vm.FuelCost,
                Status           = "Planned",
                CreatedAt        = DateTime.Now,

                PartyId          = vm.PartyId,
                LrNumber         = vm.LrNumber,
                VehicleType      = vm.VehicleType,
                MaterialName     = vm.MaterialName,
                GrossWeight      = vm.GrossWeight,
                TareWeight       = vm.TareWeight,
                FreightAmount    = vm.FreightAmount,
                AdvanceAmount    = vm.AdvanceAmount
            };

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Trip {trip.TripNumber} created.";
            return RedirectToAction(nameof(Details), new { id = trip.TripId });
        }

        // ═══════════ START TRIP ═══════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int id)
        {
            var trip = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .FirstOrDefaultAsync(t => t.TripId == id);

            if (trip == null) return NotFound();

            if (trip.Status != "Planned")
            {
                TempData["Error"] = "Sirf 'Planned' trip start ho sakta hai.";
                return RedirectToAction(nameof(Details), new { id });
            }

            trip.Status      = "InProgress";
            trip.ActualStart = DateTime.Now;

            var location = await _context.VehicleLocations
                .FirstOrDefaultAsync(vl => vl.VehicleId == trip.VehicleId);

            if (location?.OdometerKm != null)
            {
                trip.StartOdometerKm = (int?)location.OdometerKm.Value;
                trip.StartAddress = location.Address ?? $"{location.Latitude}, {location.Longitude}";
            }
            else if (trip.Vehicle != null && trip.Vehicle.CurrentOdometerKm.HasValue)
            {
                trip.StartOdometerKm = trip.Vehicle.CurrentOdometerKm.Value;
                trip.StartAddress = $"Depot: {trip.Vehicle.PlantLocation ?? "—"}";
            }
            else
            {
                trip.StartOdometerKm = 0;
            }

            if (trip.Driver != null) trip.Driver.Status = "OnTrip";
            if (trip.Vehicle != null) trip.Vehicle.Status = "Active";

            if (trip.VehicleId > 0)
            {
                var device = await _context.GpsDevices
                    .FirstOrDefaultAsync(d => d.VehicleId == trip.VehicleId && d.Status == "Active");
                if (device != null && location != null)
                {
                    location.CurrentTripId = trip.TripId;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Trip {trip.TripNumber} started. GPS tracking live hai.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ═══════════ COMPLETE TRIP ═══════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(t => t.Vehicle)
                    .Include(t => t.Driver)
                    .FirstOrDefaultAsync(t => t.TripId == id);

                if (trip == null) return NotFound();

                var status = (trip.Status ?? "").Trim();
                if (status != "InProgress" && status != "Delayed")
                {
                    TempData["Error"] = "Sirf 'InProgress' ya 'Delayed' trip complete ho sakta hai.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                trip.Status    = "Completed";
                trip.ActualEnd = DateTime.Now;

                var location = await _context.VehicleLocations
                    .FirstOrDefaultAsync(vl => vl.VehicleId == trip.VehicleId);

                if (location != null)
                {
                    trip.EndOdometerKm = (int?)(location.OdometerKm ?? 0);
                    trip.EndAddress    = location.Address ?? $"{location.Latitude}, {location.Longitude}";
                    location.CurrentTripId = null;
                }

                var gpsLogs = await _context.GpsLogs
                    .Where(g => g.TripId == id)
                    .OrderBy(g => g.RecordedAt)
                    .ToListAsync();

                if (gpsLogs.Count >= 2)
                {
                    decimal totalKm = 0;
                    for (int i = 1; i < gpsLogs.Count; i++)
                    {
                        totalKm += HaversineKm(
                            gpsLogs[i - 1].Latitude, gpsLogs[i - 1].Longitude,
                            gpsLogs[i].Latitude,     gpsLogs[i].Longitude);
                    }
                    trip.GpsDistanceKm = Math.Round(totalKm, 2);

                    var maxSpeed = gpsLogs.Where(g => g.SpeedKmph.HasValue).Select(g => g.SpeedKmph!.Value).DefaultIfEmpty(0).Max();
                    trip.GpsMaxSpeedKmph = (int?)maxSpeed;

                    var idleCount = gpsLogs.Count(g => (g.SpeedKmph == null || g.SpeedKmph == 0) && g.Ignition == 1);
                    trip.GpsIdleMinutes = idleCount / 2;
                }

                if (trip.Driver != null) trip.Driver.Status = "Available";
                if (trip.Vehicle != null) trip.Vehicle.Status = "Active";

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Trip {trip.TripNumber} completed. Distance: {trip.GpsDistanceKm ?? 0:N0} km.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Complete failed: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // ═══════════ CANCEL TRIP ═══════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancel(int id)
        {
            var trip = await _context.Trips
                .Include(t => t.Driver)
                .Include(t => t.Vehicle)
                .FirstOrDefaultAsync(t => t.TripId == id);

            if (trip == null) return NotFound();

            if (trip.Status == "Completed")
            {
                TempData["Error"] = "Completed trip cancel nahi ho sakta.";
                return RedirectToAction(nameof(Details), new { id });
            }

            trip.Status = "Cancelled";
            if (trip.Driver != null && trip.Driver.Status == "OnTrip") trip.Driver.Status = "Available";
            if (trip.Vehicle != null && trip.Vehicle.Status == "Active") trip.Vehicle.Status = "Idle";

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Trip {trip.TripNumber} cancelled.";
            return RedirectToAction(nameof(Index));
        }

        // ═══════════ HELPER — dropdowns ═══════════
        private async Task PopulateDropdowns(TripFormViewModel vm)
        {
            var busyVehicleIds = await _context.Trips
                .Where(t => t.Status == "InProgress" || t.Status == "Planned")
                .Select(t => t.VehicleId)
                .ToListAsync();

            vm.AvailableVehicles = await _context.Vehicles
                .Where(v => (v.Status == "Active" || v.Status == "Idle") && !busyVehicleIds.Contains(v.VehicleId))
                .OrderBy(v => v.RegistrationNumber)
                .Select(v => new TripVehicleOption
                {
                    VehicleId = v.VehicleId,
                    Label = v.RegistrationNumber + " — " + v.VehicleType
                })
                .ToListAsync();

            var busyDriverIds = await _context.Trips
                .Where(t => t.Status == "InProgress" || t.Status == "Planned")
                .Select(t => t.DriverId)
                .ToListAsync();

            vm.AvailableDrivers = await _context.Drivers
                .Where(d => d.Status == "Available" && !busyDriverIds.Contains(d.DriverId))
                .OrderBy(d => d.FullName)
                .Select(d => new TripDriverOption
                {
                    DriverId = d.DriverId,
                    Label = d.FullName + " (" + (d.LicenseNumber ?? "—") + ")"
                })
                .ToListAsync();

            vm.AvailableParties = await _context.Parties
                .Where(p => p.IsActive)
                .OrderBy(p => p.PartyName)
                .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = p.PartyId.ToString(),
                    Text = p.PartyName
                })
                .ToListAsync();
        }

        private static decimal HaversineKm(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const double R = 6371;
            var dLat = ToRad((double)(lat2 - lat1));
            var dLon = ToRad((double)(lon2 - lon1));
            var lat1Rad = ToRad((double)lat1);
            var lat2Rad = ToRad((double)lat2);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1Rad) * Math.Cos(lat2Rad);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return (decimal)(R * c);
        }

        private static double ToRad(double deg) => deg * Math.PI / 180;
    }
}