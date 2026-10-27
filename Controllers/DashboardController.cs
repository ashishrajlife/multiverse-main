using System.Security.Claims;
using ERPDemo.Data;
using ERPDemo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPDemo.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(AppDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("RoleName")
                       ?? User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(role))
                return RedirectToAction("Login", "Auth");

            return role switch
            {
                "Admin"   => RedirectToAction(nameof(AdminDashboard)),
                "Manager" => RedirectToAction("Dashboard", "Manager"),
                "HOD"     => RedirectToAction("Dashboard", "Hod"),
                _         => RedirectToAction(nameof(UserDashboard))
            };
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var today      = DateTime.Today;
            var tomorrow   = today.AddDays(1);
            var in30Days   = today.AddDays(30);

            var vm = new AdminDashboardViewModel();

            // ===== FLEET =====
            vm.TotalVehicles         = await _context.Vehicles.CountAsync();
            vm.ActiveVehicles        = await _context.Vehicles.CountAsync(v => v.Status == "Active");
            vm.VehiclesInMaintenance = await _context.Vehicles.CountAsync(v => v.Status == "Maintenance");
            vm.IdleVehicles          = await _context.Vehicles.CountAsync(v => v.Status == "Idle");

            vm.FleetUtilizationPct = vm.TotalVehicles == 0
                ? 0
                : Math.Round((decimal)vm.ActiveVehicles * 100 / vm.TotalVehicles, 1);

            // ===== TRIPS =====
            vm.TripsToday = await _context.Trips
                .CountAsync(t => t.CreatedAt >= today && t.CreatedAt < tomorrow);

            vm.TripsInProgress = await _context.Trips
                .CountAsync(t => t.Status == "InProgress");

            vm.TripsDelayed = await _context.Trips
                .CountAsync(t => t.Status == "Delayed");

            vm.TripsCompletedToday = await _context.Trips
                .CountAsync(t => t.Status == "Completed"
                              && t.ActualEnd >= today && t.ActualEnd < tomorrow);

            // ===== DRIVERS =====
            vm.TotalDrivers     = await _context.Drivers.CountAsync();
            vm.DriversOnDuty    = await _context.Drivers.CountAsync(d => d.Status == "OnTrip");
            vm.DriversAvailable = await _context.Drivers.CountAsync(d => d.Status == "Available");

            // ===== FINANCE =====
            vm.RevenueToday = await _context.Trips
                .Where(t => t.Status == "Completed"
                         && t.ActualEnd >= today && t.ActualEnd < tomorrow)
                .SumAsync(t => (decimal?)t.Revenue) ?? 0m;

            vm.FuelCostToday = await _context.FuelLogs
                .Where(f => f.FilledAt >= today && f.FilledAt < tomorrow)
                .SumAsync(f => (decimal?)f.Amount) ?? 0m;

            // Placeholder — hook into invoices module later
            vm.Outstanding = 0m;

            // ===== COMPLIANCE (Haryana RTO) =====
            vm.InsuranceExpiringSoon = await _context.Vehicles
                .CountAsync(v => v.InsuranceExpiry != null
                              && v.InsuranceExpiry >= today
                              && v.InsuranceExpiry <= in30Days);

            vm.FitnessExpiringSoon = await _context.Vehicles
                .CountAsync(v => v.FitnessExpiry != null
                              && v.FitnessExpiry >= today
                              && v.FitnessExpiry <= in30Days);

            vm.PermitExpiringSoon = await _context.Vehicles
                .CountAsync(v => v.PermitExpiry != null
                              && v.PermitExpiry >= today
                              && v.PermitExpiry <= in30Days);

            vm.PucExpiringSoon = await _context.Vehicles
                .CountAsync(v => v.PucExpiry != null
                              && v.PucExpiry >= today
                              && v.PucExpiry <= in30Days);

            vm.LicenseExpiringSoon = await _context.Drivers
                .CountAsync(d => d.LicenseExpiry != null
                              && d.LicenseExpiry >= today
                              && d.LicenseExpiry <= in30Days);

            // ===== USERS =====
            vm.TotalUsers  = await _context.Users.CountAsync();
            vm.ActiveUsers = await _context.Users.CountAsync(u => u.IsActive);

            // ===== LISTS =====
            vm.ActiveTrips = await _context.Trips
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Where(t => t.Status == "InProgress" || t.Status == "Delayed")
                .OrderByDescending(t => t.PlannedStart)
                .Take(8)
                .Select(t => new TripRow
                {
                    TripNumber = t.TripNumber,
                    VehicleReg = t.Vehicle != null ? t.Vehicle.RegistrationNumber : "—",
                    DriverName = t.Driver != null ? t.Driver.FullName : "—",
                    Route      = (t.OriginCity ?? "—") + " → " + (t.DestinationCity ?? "—"),
                    Status     = t.Status,
                    PlannedEnd = t.PlannedEnd,
                    Revenue    = t.Revenue
                })
                .ToListAsync();

            vm.VehiclesInService = await _context.MaintenanceLogs
                .Include(m => m.Vehicle)
                .Where(m => m.Status != "Completed")
                .OrderByDescending(m => m.ServiceDate)
                .Take(6)
                .Select(m => new MaintenanceRow
                {
                    VehicleReg  = m.Vehicle != null ? m.Vehicle.RegistrationNumber : "—",
                    ServiceType = m.ServiceType,
                    ServiceDate = m.ServiceDate,
                    Cost        = m.Cost,
                    Status      = m.Status
                })
                .ToListAsync();

            vm.RecentUsers = await _context.Users
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

            vm.TopDrivers = await _context.Trips
                .Include(t => t.Driver)
                .Where(t => t.Status == "Completed" && t.Driver != null)
                .GroupBy(t => new { t.DriverId, t.Driver!.FullName })
                .Select(g => new DriverPerf
                {
                    Name           = g.Key.FullName,
                    TripsCompleted = g.Count(),
                    Revenue        = g.Sum(x => (decimal?)x.Revenue) ?? 0m
                })
                .OrderByDescending(x => x.TripsCompleted)
                .Take(5)
                .ToListAsync();

            vm.TopVehicles = await _context.Trips
                .Include(t => t.Vehicle)
                .Where(t => t.Status == "Completed" && t.Vehicle != null)
                .GroupBy(t => new { t.VehicleId, t.Vehicle!.RegistrationNumber })
                .Select(g => new VehiclePerf
                {
                    RegistrationNumber = g.Key.RegistrationNumber,
                    TripsCompleted     = g.Count(),
                    Revenue            = g.Sum(x => (decimal?)x.Revenue) ?? 0m
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync();

            return View(vm);
        }

        [Authorize(Roles = "User,Manager,Admin,HOD")]
        public async Task<IActionResult> UserDashboard()
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idStr, out var userId))
                return RedirectToAction("Login", "Auth");

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return RedirectToAction("Login", "Auth");

            ViewBag.User = user;
            return View();
        }
    }
}