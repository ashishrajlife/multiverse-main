namespace ERPDemo.ViewModels
{
    public class ManageAllViewModel
    {
        public List<ManageRow> Members { get; set; } = new();
        public List<DepartmentOption> Departments { get; set; } = new();

        public string? SearchTerm { get; set; }
        public string? RoleFilter { get; set; }
        public string? DepartmentFilter { get; set; }
        public string? StatusFilter { get; set; }

        public int TotalMembers { get; set; }
        public int ManagerCount { get; set; }
        public int EmployeeCount { get; set; }
        public int ActiveCount { get; set; }
    }

    public class ManageRow
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class DepartmentOption
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}