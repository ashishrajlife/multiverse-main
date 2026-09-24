using ERPDemo.Data;
using ERPDemo.Filters;
using ERPDemo.Models;
using ERPDemo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ERPDemo.Controllers
{
    [Authorize(Roles = "Manager,HOD,Admin")]
    [NoCache]
    public class RequisitionsController : Controller
    {
        private readonly AppDbContext _context;

        public RequisitionsController(AppDbContext context)
        {
            _context = context;
        }

        private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        private string CurrentRole => User.FindFirst(ClaimTypes.Role)?.Value ?? "";

        private async Task<User?> GetCurrentUserAsync()
        {
            return await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
        }

        // ============ INDEX (List) ============
        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? status, string? priority)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Auth");

            var query = _context.Requisitions
                .Include(r => r.RequestedByUser)
                .Include(r => r.Department)
                .Include(r => r.Items)
                .AsQueryable();

            // Role-based filtering
            if (CurrentRole == "Manager")
            {
                // Manager sirf apni requisitions dekhe
                query = query.Where(r => r.RequestedByUserId == CurrentUserId);
            }
            else if (CurrentRole == "HOD")
            {
                // HOD apne department ki saari dekhe
                query = query.Where(r => r.DepartmentId == user.DepartmentId);
            }
            // Admin — saari dekhe

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(r =>
                    r.RequisitionNumber.ToLower().Contains(s) ||
                    r.Title.ToLower().Contains(s) ||
                    (r.Description != null && r.Description.ToLower().Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.Status == status);

            if (!string.IsNullOrWhiteSpace(priority))
                query = query.Where(r => r.Priority == priority);

            var rows = await query
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RequisitionRow
                {
                    RequisitionId = r.RequisitionId,
                    RequisitionNumber = r.RequisitionNumber,
                    Title = r.Title,
                    RequestedByName = r.RequestedByUser != null ? (r.RequestedByUser.FullName ?? r.RequestedByUser.Username) : null,
                    DepartmentName = r.Department != null ? r.Department.Name : null,
                    Priority = r.Priority,
                    RequiredByDate = r.RequiredByDate,
                    TotalEstimatedAmount = r.TotalEstimatedAmount,
                    Status = r.Status,
                    ItemCount = r.Items.Count,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

          var vm = new RequisitionListViewModel
{
    Requisitions = rows,
    SearchTerm = search,
    StatusFilter = status,
    PriorityFilter = priority,
    TotalRequisitions = rows.Count,
    DraftCount = rows.Count(r => r.Status == "Draft"),
    PendingCount = rows.Count(r => r.Status == "PendingHOD"),
    PendingAdminCount = rows.Count(r => r.Status == "PendingAdmin"),   // 👈 NAYA
    ApprovedCount = rows.Count(r => r.Status == "Approved"),           // 👈 UPDATED
    RejectedCount = rows.Count(r => r.Status == "Rejected")
};

            return View(vm);
        }

        // ============ CREATE ============
        [HttpGet]
        [Authorize(Roles = "Manager,HOD,Admin")]
        public async Task<IActionResult> Create()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Auth");

            var vm = new RequisitionFormViewModel
            {
                DepartmentId = user.DepartmentId,
                Items = new List<RequisitionItemForm> { new RequisitionItemForm() }   // ek khaali row
            };

            ViewBag.DepartmentName = user.Department?.Name ?? "Unassigned";
            ViewBag.LockedDepartment = (CurrentRole != "Admin");   // Admin change kar sakta hai
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RequisitionFormViewModel model, string action)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return RedirectToAction("Login", "Auth");

            // Force department for non-admin
            if (CurrentRole != "Admin")
                model.DepartmentId = user.DepartmentId;

            // Remove empty item rows
            model.Items = model.Items
                .Where(i => !string.IsNullOrWhiteSpace(i.ItemName))
                .ToList();

            if (model.Items.Count == 0)
            {
                ModelState.AddModelError("", "Kam se kam ek item add karo.");
            }

            // Calculate totals
            model.TotalEstimatedAmount = model.Items.Sum(i => i.Quantity * i.EstimatedRate);

            if (!ModelState.IsValid)
            {
                ViewBag.DepartmentName = user.Department?.Name ?? "Unassigned";
                ViewBag.LockedDepartment = (CurrentRole != "Admin");
                return View(model);
            }

            // Generate RequisitionNumber
            var currentYear = DateTime.Now.Year;
            var lastReq = await _context.Requisitions
                .Where(r => r.CreatedAt.Year == currentYear)
                .OrderByDescending(r => r.RequisitionId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastReq != null)
            {
                var parts = lastReq.RequisitionNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out var lastNum))
                    nextNumber = lastNum + 1;
            }
            string reqNumber = $"REQ-{currentYear}-{nextNumber:D4}";

            // Status based on action
            string status = (action == "submit") ? "PendingHOD" : "Draft";

            var requisition = new Requisition
            {
                RequisitionNumber = reqNumber,
                Title = model.Title,
                Description = model.Description,
                RequestedByUserId = CurrentUserId,
                DepartmentId = model.DepartmentId,
                Priority = model.Priority,
                RequiredByDate = model.RequiredByDate,
                Status = status,
                TotalEstimatedAmount = model.TotalEstimatedAmount,
                CreatedAt = DateTime.Now,
                Items = model.Items.Select(i => new RequisitionItem
                {
                    ItemName = i.ItemName,
                    Description = i.Description,
                    Category = i.Category,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    EstimatedRate = i.EstimatedRate,
                    EstimatedAmount = i.Quantity * i.EstimatedRate,
                    Remarks = i.Remarks
                }).ToList()
            };

            _context.Requisitions.Add(requisition);
            await _context.SaveChangesAsync();

            TempData["Success"] = action == "submit"
                ? $"'{reqNumber}' HOD ko bhej di gayi."
                : $"'{reqNumber}' draft ke roop mein save ho gayi.";

            return RedirectToAction(nameof(Index));
        }

        // ============ EDIT ============
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var requisition = await _context.Requisitions
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.RequisitionId == id);

            if (requisition == null) return NotFound();

            // Sirf Draft edit ho sakti hai
            if (requisition.Status != "Draft")
            {
                TempData["Error"] = "Sirf Draft requisitions edit ho sakti hain.";
                return RedirectToAction(nameof(Index));
            }

            // Sirf owner edit kare
            if (requisition.RequestedByUserId != CurrentUserId && CurrentRole != "Admin")
            {
                TempData["Error"] = "Aap is requisition ko edit nahi kar sakte.";
                return RedirectToAction(nameof(Index));
            }

            var user = await GetCurrentUserAsync();
            var vm = new RequisitionFormViewModel
            {
                RequisitionId = requisition.RequisitionId,
                Title = requisition.Title,
                Description = requisition.Description,
                Priority = requisition.Priority,
                RequiredByDate = requisition.RequiredByDate,
                DepartmentId = requisition.DepartmentId,
                TotalEstimatedAmount = requisition.TotalEstimatedAmount,
                Items = requisition.Items.Select(i => new RequisitionItemForm
                {
                    RequisitionItemId = i.RequisitionItemId,
                    ItemName = i.ItemName,
                    Description = i.Description,
                    Category = i.Category,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    EstimatedRate = i.EstimatedRate,
                    EstimatedAmount = i.EstimatedAmount,
                    Remarks = i.Remarks
                }).ToList()
            };

            ViewBag.DepartmentName = user?.Department?.Name ?? "Unassigned";
            ViewBag.LockedDepartment = (CurrentRole != "Admin");
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RequisitionFormViewModel model, string action)
        {
            var requisition = await _context.Requisitions
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.RequisitionId == model.RequisitionId);

            if (requisition == null) return NotFound();
            if (requisition.Status != "Draft") return RedirectToAction(nameof(Index));

            var user = await GetCurrentUserAsync();

            if (CurrentRole != "Admin")
                model.DepartmentId = user?.DepartmentId;

            model.Items = model.Items
                .Where(i => !string.IsNullOrWhiteSpace(i.ItemName))
                .ToList();

            if (model.Items.Count == 0)
                ModelState.AddModelError("", "Kam se kam ek item add karo.");

            model.TotalEstimatedAmount = model.Items.Sum(i => i.Quantity * i.EstimatedRate);

            if (!ModelState.IsValid)
            {
                ViewBag.DepartmentName = user?.Department?.Name ?? "Unassigned";
                ViewBag.LockedDepartment = (CurrentRole != "Admin");
                return View(model);
            }

            // Update
            requisition.Title = model.Title;
            requisition.Description = model.Description;
            requisition.Priority = model.Priority;
            requisition.RequiredByDate = model.RequiredByDate;
            requisition.DepartmentId = model.DepartmentId;
            requisition.TotalEstimatedAmount = model.TotalEstimatedAmount;
            requisition.UpdatedAt = DateTime.Now;
            requisition.Status = (action == "submit") ? "PendingHOD" : "Draft";

            // Remove old items
            _context.RequisitionItems.RemoveRange(requisition.Items);

            // Add new items
            requisition.Items = model.Items.Select(i => new RequisitionItem
            {
                RequisitionId = requisition.RequisitionId,
                ItemName = i.ItemName,
                Description = i.Description,
                Category = i.Category,
                Quantity = i.Quantity,
                Unit = i.Unit,
                EstimatedRate = i.EstimatedRate,
                EstimatedAmount = i.Quantity * i.EstimatedRate,
                Remarks = i.Remarks
            }).ToList();

            await _context.SaveChangesAsync();

            TempData["Success"] = action == "submit"
                ? "Requisition HOD ko bhej di gayi."
                : "Requisition update ho gayi.";

            return RedirectToAction(nameof(Index));
        }

        // ============ DETAILS ============
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var requisition = await _context.Requisitions
                .Include(r => r.Items)
                .Include(r => r.RequestedByUser)
                .Include(r => r.Department)
                .Include(r => r.HodApprovedByUser)
                  .Include(r => r.AdminApprovedByUser)
                .FirstOrDefaultAsync(r => r.RequisitionId == id);

            if (requisition == null) return NotFound();

            var user = await GetCurrentUserAsync();

            // Permission check
            if (CurrentRole == "Manager" && requisition.RequestedByUserId != CurrentUserId)
            {
                TempData["Error"] = "Aap is requisition ko dekh nahi sakte.";
                return RedirectToAction(nameof(Index));
            }
            if (CurrentRole == "HOD" && requisition.DepartmentId != user?.DepartmentId)
            {
                TempData["Error"] = "Aap is requisition ko dekh nahi sakte.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new RequisitionDetailsViewModel
            {
                RequisitionId = requisition.RequisitionId,
                RequisitionNumber = requisition.RequisitionNumber,
                Title = requisition.Title,
                Description = requisition.Description,
                RequestedByName = requisition.RequestedByUser?.FullName ?? requisition.RequestedByUser?.Username,
                RequestedByEmail = requisition.RequestedByUser?.Email,
                DepartmentName = requisition.Department?.Name,
                Priority = requisition.Priority,
                RequiredByDate = requisition.RequiredByDate,
                Status = requisition.Status,
                TotalEstimatedAmount = requisition.TotalEstimatedAmount,
                HodRemarks = requisition.HodRemarks,
                RejectionReason = requisition.RejectionReason,
                HodApprovedAt = requisition.HodApprovedAt,
                HodApprovedByName = requisition.HodApprovedByUser?.FullName ?? requisition.HodApprovedByUser?.Username,
                CreatedAt = requisition.CreatedAt,
                UpdatedAt = requisition.UpdatedAt,
                Items = requisition.Items.Select(i => new RequisitionItemForm
                {
                    RequisitionItemId = i.RequisitionItemId,
                    ItemName = i.ItemName,
                    Description = i.Description,
                    Category = i.Category,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    EstimatedRate = i.EstimatedRate,
                    EstimatedAmount = i.EstimatedAmount,
                    Remarks = i.Remarks
                }).ToList()
            };

           // Permissions
vm.CanEdit = requisition.Status == "Draft"
           && requisition.RequestedByUserId == CurrentUserId;

vm.CanSubmit = vm.CanEdit;

// HOD can approve ONLY if PendingHOD
// Admin can approve ONLY if PendingAdmin
vm.CanApprove = (CurrentRole == "HOD" && requisition.Status == "PendingHOD"
                 || CurrentRole == "Admin" && requisition.Status == "PendingAdmin")
             && requisition.RequestedByUserId != CurrentUserId;

vm.CanReject = vm.CanApprove;

vm.CanDelete = requisition.Status == "Draft"
            && requisition.RequestedByUserId == CurrentUserId;

// Naye fields VM mein
vm.AdminApprovedByName = requisition.AdminApprovedByUser?.FullName ?? requisition.AdminApprovedByUser?.Username;
vm.AdminApprovedAt = requisition.AdminApprovedAt;
vm.AdminRemarks = requisition.AdminRemarks;

            return View(vm);
        }

        // ============ SUBMIT ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var requisition = await _context.Requisitions.FindAsync(id);
            if (requisition == null) return NotFound();

            if (requisition.RequestedByUserId != CurrentUserId && CurrentRole != "Admin")
                return Forbid();

            if (requisition.Status != "Draft")
            {
                TempData["Error"] = "Sirf Draft submit kar sakte ho.";
                return RedirectToAction(nameof(Details), new { id });
            }

            requisition.Status = "PendingHOD";
            requisition.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{requisition.RequisitionNumber}' HOD ko bhej di gayi.";
            return RedirectToAction(nameof(Details), new { id });
        }

       // ============ APPROVE (HOD Level 1, Admin Level 2) ============
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "HOD,Admin")]
public async Task<IActionResult> Approve(int id, string? remarks)
{
    var requisition = await _context.Requisitions.FindAsync(id);
    if (requisition == null) return NotFound();

    if (requisition.RequestedByUserId == CurrentUserId)
    {
        TempData["Error"] = "Aap apni requisition approve nahi kar sakte.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ============ HOD APPROVAL (Level 1) ============
    if (CurrentRole == "HOD")
    {
        if (requisition.Status != "PendingHOD")
        {
            TempData["Error"] = "Yeh requisition HOD approval ke liye pending nahi hai.";
            return RedirectToAction(nameof(Details), new { id });
        }

        requisition.Status = "PendingAdmin";   // 👈 NAYA STATUS
        requisition.HodApprovedByUserId = CurrentUserId;
        requisition.HodApprovedAt = DateTime.Now;
        requisition.HodRemarks = remarks;
        requisition.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["Success"] = $"'{requisition.RequisitionNumber}' HOD ne approve kar di. Ab Admin approval ke liye pending hai.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ============ ADMIN APPROVAL (Level 2 — Final) ============
    if (CurrentRole == "Admin")
    {
        if (requisition.Status != "PendingAdmin")
        {
            TempData["Error"] = "Yeh requisition Admin approval ke liye pending nahi hai.";
            return RedirectToAction(nameof(Details), new { id });
        }

        requisition.Status = "Approved";   // 👈 FINAL STATUS
        requisition.HodApprovedByUserId = requisition.HodApprovedByUserId;  // preserve
        requisition.AdminApprovedByUserId = CurrentUserId;   // 👈 NAYA FIELD
        requisition.AdminApprovedAt = DateTime.Now;           // 👈 NAYA FIELD
        requisition.AdminRemarks = remarks;                   // 👈 NAYA FIELD
        requisition.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["Success"] = $"'{requisition.RequisitionNumber}' Admin ne final approve kar di.";
        return RedirectToAction(nameof(Details), new { id });
    }

    TempData["Error"] = "Aap is action ko perform nahi kar sakte.";
    return RedirectToAction(nameof(Details), new { id });
}

       [HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "HOD,Admin")]
public async Task<IActionResult> Reject(int id, string reason)
{
    var requisition = await _context.Requisitions.FindAsync(id);
    if (requisition == null) return NotFound();

    if (string.IsNullOrWhiteSpace(reason))
    {
        TempData["Error"] = "Reject karne ke liye reason dena zaroori hai.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // HOD sirf PendingHOD reject kar sakta hai
    if (CurrentRole == "HOD" && requisition.Status != "PendingHOD")
    {
        TempData["Error"] = "Yeh requisition HOD reject karne ke liye pending nahi hai.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // Admin sirf PendingAdmin reject kar sakta hai
    if (CurrentRole == "Admin" && requisition.Status != "PendingAdmin")
    {
        TempData["Error"] = "Yeh requisition Admin reject karne ke liye pending nahi hai.";
        return RedirectToAction(nameof(Details), new { id });
    }

    requisition.Status = "Rejected";
    requisition.RejectionReason = reason;
    requisition.UpdatedAt = DateTime.Now;

    if (CurrentRole == "HOD")
    {
        requisition.HodApprovedByUserId = CurrentUserId;
        requisition.HodApprovedAt = DateTime.Now;
    }
    else if (CurrentRole == "Admin")
    {
        requisition.AdminApprovedByUserId = CurrentUserId;
        requisition.AdminApprovedAt = DateTime.Now;
    }

    await _context.SaveChangesAsync();

    TempData["Success"] = $"'{requisition.RequisitionNumber}' reject kar di gayi.";
    return RedirectToAction(nameof(Details), new { id });
}

       [HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "HOD,Admin")]
public async Task<IActionResult> Return(int id, string reason)
{
    var requisition = await _context.Requisitions.FindAsync(id);
    if (requisition == null) return NotFound();

    // HOD sirf PendingHOD return kar sakta hai
    if (CurrentRole == "HOD" && requisition.Status != "PendingHOD")
    {
        TempData["Error"] = "Yeh requisition return karne ke liye pending nahi hai.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // Admin sirf PendingAdmin return kar sakta hai
    if (CurrentRole == "Admin" && requisition.Status != "PendingAdmin")
    {
        TempData["Error"] = "Yeh requisition return karne ke liye pending nahi hai.";
        return RedirectToAction(nameof(Details), new { id });
    }

    requisition.Status = "Draft";
    requisition.HodRemarks = reason;
    requisition.UpdatedAt = DateTime.Now;

    await _context.SaveChangesAsync();

    TempData["Success"] = $"'{requisition.RequisitionNumber}' Manager ko wapas bhej di gayi.";
    return RedirectToAction(nameof(Details), new { id });
}

        // ============ DELETE ============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var requisition = await _context.Requisitions
                .Include(r => r.Items)
                .FirstOrDefaultAsync(r => r.RequisitionId == id);

            if (requisition == null) return NotFound();

            if (requisition.Status != "Draft")
            {
                TempData["Error"] = "Sirf Draft requisitions delete ho sakti hain.";
                return RedirectToAction(nameof(Index));
            }

            if (requisition.RequestedByUserId != CurrentUserId && CurrentRole != "Admin")
            {
                TempData["Error"] = "Aap is requisition ko delete nahi kar sakte.";
                return RedirectToAction(nameof(Index));
            }

            _context.RequisitionItems.RemoveRange(requisition.Items);
            _context.Requisitions.Remove(requisition);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{requisition.RequisitionNumber}' delete ho gayi.";
            return RedirectToAction(nameof(Index));
        }

       [HttpGet]
[Authorize(Roles = "HOD,Admin")]
public async Task<IActionResult> PendingApprovals()
{
    var user = await GetCurrentUserAsync();
    if (user == null) return RedirectToAction("Login", "Auth");

    var query = _context.Requisitions
        .Include(r => r.RequestedByUser)
        .Include(r => r.Department)
        .Include(r => r.Items)
        .AsQueryable();

    // HOD → PendingHOD dekhe apne dept ki
    if (CurrentRole == "HOD")
    {
        query = query.Where(r => r.Status == "PendingHOD"
                              && r.DepartmentId == user.DepartmentId);
    }
    // Admin → PendingAdmin dekhe saari
    else if (CurrentRole == "Admin")
    {
        query = query.Where(r => r.Status == "PendingAdmin");
    }

    var rows = await query
        .OrderByDescending(r => r.Priority == "Urgent")
        .ThenByDescending(r => r.CreatedAt)
        .Select(r => new RequisitionRow
        {
            RequisitionId = r.RequisitionId,
            RequisitionNumber = r.RequisitionNumber,
            Title = r.Title,
            RequestedByName = r.RequestedByUser != null ? (r.RequestedByUser.FullName ?? r.RequestedByUser.Username) : null,
            DepartmentName = r.Department != null ? r.Department.Name : null,
            Priority = r.Priority,
            RequiredByDate = r.RequiredByDate,
            TotalEstimatedAmount = r.TotalEstimatedAmount,
            Status = r.Status,
            ItemCount = r.Items.Count,
            CreatedAt = r.CreatedAt
        })
        .ToListAsync();

    var vm = new RequisitionListViewModel
    {
        Requisitions = rows,
        TotalRequisitions = rows.Count,
        PendingCount = rows.Count
    };

    return View(vm);
}
    }
}