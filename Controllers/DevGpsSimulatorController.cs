using ERPDemo.Data;
using ERPDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPDemo.Controllers
{
    /// <summary>
    /// DEV-ONLY: Fake GPS data injector.
    /// Sirf Development environment mein kaam karta hai.
    /// </summary>
    [AllowAnonymous]
    public class DevGpsSimulatorController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DevGpsSimulatorController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ═══════════ GET: Simulator main page ═══════════
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!_env.IsDevelopment())
                return NotFound("Simulator only runs in Development mode.");

            var activeTrips = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Where(t => t.Status == "InProgress" || t.Status == "Planned")
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.ActiveTrips = activeTrips;
            return View();
        }

        // ═══════════ GET: Simulation form ═══════════
        [HttpGet]
        public async Task<IActionResult> SimulateTripForm(int vehicleId, int tripId)
        {
            if (!_env.IsDevelopment())
                return NotFound();

            var trip = await _context.Trips.FindAsync(tripId);
            if (trip == null) return NotFound($"Trip {tripId} not found.");

            ViewBag.VehicleId   = vehicleId;
            ViewBag.TripId      = tripId;
            ViewBag.TripNumber  = trip.TripNumber;
            ViewBag.Origin      = trip.OriginCity ?? "Gurugram";
            ViewBag.Destination = trip.DestinationCity ?? "Jaipur";

            return View();
        }

        // ═══════════ POST: Run simulation from form ═══════════
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunSimulation(
            int vehicleId, int tripId,
            decimal fromLat, decimal fromLng,
            decimal toLat, decimal toLng,
            int points = 30)
        {
            if (!_env.IsDevelopment())
                return NotFound();

            await SimulateTrip(vehicleId, tripId, fromLat, fromLng, toLat, toLng, points);

            TempData["Success"] = $"{points} GPS points inject ho gaye. Map check karo!";
            return RedirectToAction("Details", "Trips", new { id = tripId });
        }

        // ═══════════ POST: Single ping ═══════════
        [HttpPost]
        public async Task<IActionResult> Simulate(
            int vehicleId, int? tripId,
            decimal lat, decimal lng,
            decimal? speed = 0, int? ignition = 1,
            decimal? odometerKm = null)
        {
            if (!_env.IsDevelopment())
                return NotFound("Simulator only runs in Development mode.");

            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null) return NotFound($"Vehicle {vehicleId} not found.");

            var device = await GetOrCreateDeviceAsync(vehicle);

            device.LastPingAt = DateTime.Now;

            var log = new GpsLog
            {
                GpsDeviceId = device.GpsDeviceId,
                VehicleId   = vehicleId,
                TripId      = tripId,
                Latitude    = lat,
                Longitude   = lng,
                SpeedKmph   = speed,
                Ignition    = ignition,
                OdometerKm  = odometerKm,
                RecordedAt  = DateTime.Now,
                ReceivedAt  = DateTime.Now,
                Source      = "simulator"
            };
            _context.GpsLogs.Add(log);

            var vLoc = await _context.VehicleLocations
                .FirstOrDefaultAsync(vl => vl.VehicleId == vehicleId);
            if (vLoc == null)
            {
                vLoc = new VehicleLocation { VehicleId = vehicleId };
                _context.VehicleLocations.Add(vLoc);
            }
            vLoc.Latitude      = lat;
            vLoc.Longitude     = lng;
            vLoc.SpeedKmph     = speed;
            vLoc.Ignition      = ignition;
            vLoc.OdometerKm    = odometerKm;
            vLoc.LastUpdatedAt = DateTime.Now;
            vLoc.CurrentTripId = tripId;

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "GPS ping accepted (simulator)",
                vehicleId, tripId, lat, lng, speed, odometerKm,
                timestamp = log.RecordedAt
            });
        }

        // ═══════════ POST: Multi-point trip route ═══════════
        [HttpPost]
        public async Task<IActionResult> SimulateTrip(
            int vehicleId, int tripId,
            decimal fromLat, decimal fromLng,
            decimal toLat, decimal toLng,
            int points = 30)
        {
            if (!_env.IsDevelopment())
                return NotFound("Simulator only runs in Development mode.");

            if (points < 2) points = 2;
            if (points > 200) points = 200;

            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null) return NotFound($"Vehicle {vehicleId} not found.");

            var device = await GetOrCreateDeviceAsync(vehicle);

            var startOdo = (int)(vehicle.CurrentOdometerKm ?? 0);
            var avgSpeed = 60m;
            var rnd = new Random();

            for (int i = 0; i < points; i++)
            {
                var t = (decimal)i / (points - 1);
                var lat = fromLat + (toLat - fromLat) * t;
                var lng = fromLng + (toLng - fromLng) * t;

                // Slight jitter for realism
                lat += (decimal)(rnd.NextDouble() - 0.5) * 0.002m;
                lng += (decimal)(rnd.NextDouble() - 0.5) * 0.002m;

                var log = new GpsLog
                {
                    GpsDeviceId = device.GpsDeviceId,
                    VehicleId   = vehicleId,
                    TripId      = tripId,
                    Latitude    = lat,
                    Longitude   = lng,
                    SpeedKmph   = avgSpeed + rnd.Next(-10, 25),
                    Ignition    = 1,
                    OdometerKm  = startOdo + (i * 5),
                    RecordedAt  = DateTime.Now.AddMinutes(-((points - 1 - i) * 2)),
                    ReceivedAt  = DateTime.Now,
                    Source      = "simulator"
                };
                _context.GpsLogs.Add(log);
            }

            var vLoc = await _context.VehicleLocations
                .FirstOrDefaultAsync(vl => vl.VehicleId == vehicleId);
            if (vLoc == null)
            {
                vLoc = new VehicleLocation { VehicleId = vehicleId };
                _context.VehicleLocations.Add(vLoc);
            }
            vLoc.Latitude      = toLat;
            vLoc.Longitude     = toLng;
            vLoc.SpeedKmph     = avgSpeed;
            vLoc.OdometerKm    = startOdo + (points * 5);
            vLoc.LastUpdatedAt = DateTime.Now;
            vLoc.CurrentTripId = tripId;

            device.LastPingAt = DateTime.Now;

            // Update vehicle odometer
            vehicle.CurrentOdometerKm = startOdo + (points * 5);

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Inserted {points} GPS points",
                vehicleId, tripId,
                from = new { lat = fromLat, lng = fromLng },
                to   = new { lat = toLat, lng = toLng },
                points
            });
        }

        // ═══════════ HELPER ═══════════
        private async Task<GpsDevice> GetOrCreateDeviceAsync(Vehicle vehicle)
        {
            var device = await _context.GpsDevices
                .FirstOrDefaultAsync(d => d.VehicleId == vehicle.VehicleId);
            if (device == null)
            {
                device = new GpsDevice
                {
                    Imei        = $"SIM-{vehicle.RegistrationNumber}-{Guid.NewGuid().ToString().Substring(0, 8)}",
                    DeviceModel = "SIMULATOR",
                    VehicleId   = vehicle.VehicleId,
                    Status      = "Active",
                    InstalledAt = DateTime.Now,
                    LastPingAt  = DateTime.Now,
                    CreatedAt   = DateTime.Now
                };
                _context.GpsDevices.Add(device);
                await _context.SaveChangesAsync();
            }
            return device;
        }
    }
}