using ERPDemo.Models;

namespace ERPDemo.ViewModels
{
    public class AdminDashboardViewModel
    {
        // Fleet
        public int TotalVehicles { get; set; }
        public int ActiveVehicles { get; set; }
        public int VehiclesInMaintenance { get; set; }
        public int IdleVehicles { get; set; }
        public decimal FleetUtilizationPct { get; set; }

        // Trips
        public int TripsToday { get; set; }
        public int TripsInProgress { get; set; }
        public int TripsDelayed { get; set; }
        public int TripsCompletedToday { get; set; }

        // Drivers
        public int TotalDrivers { get; set; }
        public int DriversOnDuty { get; set; }
        public int DriversAvailable { get; set; }

        // Finance
        public decimal RevenueToday { get; set; }
        public decimal FuelCostToday { get; set; }
        public decimal Outstanding { get; set; }

        // Compliance (Haryana)
        public int InsuranceExpiringSoon { get; set; }   // within 30 days
        public int FitnessExpiringSoon { get; set; }
        public int PermitExpiringSoon { get; set; }
        public int PucExpiringSoon { get; set; }
        public int LicenseExpiringSoon { get; set; }

        // Users
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }

        // Lists
        public List<TripRow> ActiveTrips { get; set; } = new();
        public List<MaintenanceRow> VehiclesInService { get; set; } = new();
        public List<User> RecentUsers { get; set; } = new();
        public List<DriverPerf> TopDrivers { get; set; } = new();
        public List<VehiclePerf> TopVehicles { get; set; } = new();
    }

    public class TripRow
    {
        public string TripNumber { get; set; } = "";
        public string VehicleReg { get; set; } = "";
        public string DriverName { get; set; } = "";
        public string Route { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime? PlannedEnd { get; set; }
        public decimal? Revenue { get; set; }
    }

    public class MaintenanceRow
    {
        public string VehicleReg { get; set; } = "";
        public string ServiceType { get; set; } = "";
        public DateTime ServiceDate { get; set; }
        public decimal? Cost { get; set; }
        public string Status { get; set; } = "";
    }

    public class DriverPerf
    {
        public string Name { get; set; } = "";
        public int TripsCompleted { get; set; }
        public decimal Revenue { get; set; }
    }

    public class VehiclePerf
    {
        public string RegistrationNumber { get; set; } = "";
        public int TripsCompleted { get; set; }
        public decimal Revenue { get; set; }
    }
}