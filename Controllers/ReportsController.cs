using ERPDemo.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPDemo.Controllers
{
    [Authorize(Roles = "Admin,Manager,HOD")]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? from, DateTime? to)
        {
            // Default: Last 30 days if no date provided
            var startDate = from ?? DateTime.Today.AddDays(-30);
            var endDate = to ?? DateTime.Today;

            // Make sure we include the full end date (e.g., to 31 Oct means upto 31 Oct 23:59:59)
            var endDateInc = endDate.Date.AddDays(1);

            // =====================================================
            // 1. TRIPS — Filter based on CREATED or ACTUAL END date
            // =====================================================
            var trips = await _context.Trips
                .Where(t => (t.CreatedAt >= startDate && t.CreatedAt < endDateInc) ||
                            (t.ActualEnd >= startDate && t.ActualEnd < endDateInc))
                .ToListAsync();

            // =====================================================
            // 2. FUEL — Filter based on FilledAt
            // =====================================================
            var fuel = await _context.FuelLogs
                .Where(f => f.FilledAt >= startDate && f.FilledAt < endDateInc)
                .ToListAsync();

            // =====================================================
            // 3. MAINTENANCE — Filter based on ServiceDate
            // =====================================================
            var maint = await _context.MaintenanceLogs
                .Where(m => m.ServiceDate >= startDate && m.ServiceDate < endDateInc)
                .ToListAsync();

            // Pass dates to view
            ViewBag.From = startDate;
            ViewBag.To = endDate;

            // =====================================================
            // KPIs
            // =====================================================
            ViewBag.TotalTrips     = trips.Count;
            ViewBag.CompletedTrips = trips.Count(t => t.Status == "Completed");
            ViewBag.TotalRevenue   = trips.Where(t => t.Status == "Completed").Sum(t => t.Revenue ?? 0);
            ViewBag.TotalDistance  = trips.Where(t => t.Status == "Completed").Sum(t => t.GpsDistanceKm ?? 0);
            ViewBag.TotalFuelCost  = fuel.Sum(f => f.Amount);
            ViewBag.TotalFuelLiters= fuel.Sum(f => f.Liters);
            ViewBag.TotalMaintCost = maint.Sum(m => m.Cost ?? 0);

            // =====================================================
            // Vehicle-wise Summary
            // =====================================================
            var vehicleSummary = await _context.Vehicles
                .Select(v => new
                {
                    v.RegistrationNumber,
                    Trips = _context.Trips.Count(t => t.VehicleId == v.VehicleId
                        && t.Status == "Completed"
                        && ((t.CreatedAt >= startDate && t.CreatedAt < endDateInc) ||
                            (t.ActualEnd >= startDate && t.ActualEnd < endDateInc))),

                    Distance = _context.Trips
                        .Where(t => t.VehicleId == v.VehicleId
                            && t.Status == "Completed"
                            && ((t.CreatedAt >= startDate && t.CreatedAt < endDateInc) ||
                                (t.ActualEnd >= startDate && t.ActualEnd < endDateInc)))
                        .Sum(t => (decimal?)t.GpsDistanceKm) ?? 0,

                    Revenue = _context.Trips
                        .Where(t => t.VehicleId == v.VehicleId
                            && t.Status == "Completed"
                            && ((t.CreatedAt >= startDate && t.CreatedAt < endDateInc) ||
                                (t.ActualEnd >= startDate && t.ActualEnd < endDateInc)))
                        .Sum(t => (decimal?)t.Revenue) ?? 0,

                    FuelCost = _context.FuelLogs
                        .Where(f => f.VehicleId == v.VehicleId
                            && f.FilledAt >= startDate && f.FilledAt < endDateInc)
                        .Sum(f => (decimal?)f.Amount) ?? 0
                })
                .Where(x => x.Trips > 0 || x.FuelCost > 0)
                .ToListAsync();

            ViewBag.VehicleSummary = vehicleSummary;

            return View();
        }
    }
}