using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Workflows.Data;
using Workflows.Models;
using Workflows.Attributes;
using Workflows.ViewModels;
using Microsoft.AspNetCore.Html;
using Workflows.Services;

namespace Workflows.Controllers
{
    [CustomAuthorize] /// Used to ensure authenticated users view this class/pages
    public class RequisitionsController : Controller
    {
        private readonly WorkflowsContext _context;
        private readonly IRelationshipService _relationshipService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequisitionsController(WorkflowsContext context, IRelationshipService relationshipService, 
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _relationshipService = relationshipService;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Requisitions
        public async Task<IActionResult> Index(RequisitionFilterViewModel filter)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userRole = httpContext.Session.GetString("EmployeeRole");
            var userPayroll = httpContext.Session.GetString("EmployeePayrollNo");
            var sessionDepartmentID = httpContext.Session.GetString("EmployeeDepartmentID");
          

            using var ktdaContext = new KtdaleaveContext();
            var departmentCode = await ktdaContext.Departments
                 .Where(d => d.DepartmentId == sessionDepartmentID)
                .Select(e => e.DepartmentCode)
                .FirstOrDefaultAsync();

            Console.WriteLine($"sessionDepartmentID: {sessionDepartmentID}");
              Console.WriteLine($"departmentCode: {departmentCode}");
            var query = _context.Requisition
            .Where(r => userRole == "Admin" || userRole == "HR" || r.DepartmentCode == departmentCode);

            ///FILTERS
            ///
            if (filter.DepartmentCode.HasValue)
            {
                query = query.Where(r => r.DepartmentCode == filter.DepartmentCode.Value);
            }
            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(r => r.Status == filter.Status);
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(r => r.Start_Date >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(r => r.End_Date <= filter.EndDate.Value);
            }

            var requisitions = await query
             .OrderByDescending(e => e.Id)
             .Take(50)
             .ToListAsync();

            // Get the list of departments from the KtdaleaveContext
            var employeeName = new Dictionary<string, string>();
            var internName = new Dictionary<int, string>();
            List<Department> departments;
            var approvalStatuses = new Dictionary<int, string>(); // RequisitionId to ApprovalStatus with pending
            var approvalEmployeeNames = new Dictionary<int, string>(); // RequisitionId to Approval EmployeeName
            var StepNumber = new Dictionary<int, int>();
            var approvalStep = new Dictionary<int, string>();
            var approvalComment = new Dictionary<int, string>();
            var today = DateTime.Today;
            var daysLeftDictionary = new Dictionary<int, int>();
           
                departments = await ktdaContext.Departments.ToListAsync();
              
                // QUery to Get Requisitions ////

                foreach (var requisition in requisitions)
                {
                    var employee = ktdaContext.EmployeeBkps.FirstOrDefault(d => d.PayrollNo == requisition.PayrollNo);
                    var intern = _context.Intern.FirstOrDefault(d =>d.Id == requisition.Intern_id);
                    // Add to the department dictionary
                    if (employee != null && !employeeName.ContainsKey(requisition.PayrollNo))
                    {
                        employeeName[requisition.PayrollNo] = employee.Fullname;
                    }
                    // Add to the intern dictionary
                    if (intern != null && !internName.ContainsKey(requisition.Intern_id))
                    {
                        internName[requisition.Intern_id] = intern.Firstname + " " + intern.Lastname;
                    }
                    // Get the remaining Days requisition has to end
                    int daysLeft = (requisition.End_Date - DateTime.Now).Days;
                    daysLeftDictionary[requisition.Id] = daysLeft > 0 ? daysLeft : 0;

                    // Retrieve the pending approval for the requisition
                    var pendingApproval = await _context.Approval
                        .Where(a => a.Requisition_id == requisition.Id && a.ApprovalStatus == "Pending")
                        .FirstOrDefaultAsync();
                    if (pendingApproval != null)
                    {
                        approvalStatuses[requisition.Id] = "Pending Approval";
                        var approvalEmployee = await ktdaContext.EmployeeBkps.FirstOrDefaultAsync(e => e.PayrollNo == pendingApproval.PayrollNo);
                        if (approvalEmployee != null)
                        {
                            approvalEmployeeNames[requisition.Id] = approvalEmployee.Fullname;
                        }
                        // Include the ApprovalComment and step
                        StepNumber[requisition.Id] = pendingApproval.StepNumber;
                        approvalStep[requisition.Id] = pendingApproval.ApprovalStep;
                        approvalComment[requisition.Id] = pendingApproval.ApprovalComment;
                    }
                    else
                    {
                        // Check for rejected approval
                        var rejectedApproval = await _context.Approval
                            .Where(a => a.Requisition_id == requisition.Id && a.ApprovalStatus == "Rejected")
                            .FirstOrDefaultAsync();

                        if (rejectedApproval != null)
                        {
                            approvalStatuses[requisition.Id] = "Approval Rejected";
                            var rejectionEmployee = await ktdaContext.EmployeeBkps.FirstOrDefaultAsync(e => e.PayrollNo == rejectedApproval.PayrollNo);
                            if (rejectionEmployee != null)
                            {
                                approvalEmployeeNames[requisition.Id] = rejectionEmployee.Fullname;
                            }
                            // Include the ApprovalComment
                            StepNumber[requisition.Id] = rejectedApproval.StepNumber;
                            approvalStep[requisition.Id] = rejectedApproval.ApprovalStep;
                            approvalComment[requisition.Id] = rejectedApproval.ApprovalComment;
                        }
                        else
                        {
                            // If no pending or rejected approval, it means the approval process is fully approved
                            approvalStatuses[requisition.Id] = "Fully Approved";
                            // No comment for fully approved requisitions
                            approvalComment[requisition.Id] = string.Empty;
                        }
                    }
                }
            

            // Pass both sets of data to the view
            ViewBag.EmployeeNames = employeeName;
            ViewBag.InternNames = internName;
            ViewBag.Departments = departments;
            ViewBag.ApprovalStatuses = approvalStatuses;
            ViewBag.ApprovalEmployeeNames = approvalEmployeeNames;
            ViewBag.StepNumber = StepNumber;
            ViewBag.ApprovalStep = approvalStep;
            ViewBag.ApprovalComment = approvalComment;
            ViewBag.DaysLeftDictionary = daysLeftDictionary;
            var viewModel = new RequisitionIndexViewModel
            {
                Filter = filter,
                Requisitions = requisitions
            };
            viewModel.Filter.StatusList = await _context.Requisition
            .Select(r => r.Status)
            .Distinct()
            .Select(s => new SelectListItem { Value = s, Text = s })
            .ToListAsync();
            viewModel.Filter.DepartmentList = await ktdaContext.Departments
              .Select(d => new SelectListItem { Value = d.DepartmentCode.ToString(), Text = d.DepartmentName })
              .ToListAsync();

            return View(viewModel);
        }

        // GET: Requisitions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requisition = await _context.Requisition.FirstOrDefaultAsync(m => m.Id == id);
            if (requisition == null)
            {
                return NotFound();
            }
            var intern = _context.Intern.FirstOrDefault(d => d.Id == requisition.Intern_id);
            var approvals = await _context.Approval.Where(a => a.Requisition_id == id).ToListAsync();
            var documents = await _context.Document.Where(d => d.Requisition_id == id).ToListAsync();
            Department department;
            EmployeeBkp addedBy;
            EmployeeBkp employee;
            using (var db = new KtdaleaveContext())
            {
                department = db.Departments.FirstOrDefault(d => d.DepartmentCode == requisition.DepartmentCode);
                addedBy = db.EmployeeBkps.FirstOrDefault(d => d.PayrollNo == requisition.PayrollNo);


                var requisitionDetails = new RequisitionDetailsViewModel
                {
                    Requisition = requisition,
                    Intern = intern,
                    Approvals = approvals.Select(approval =>
                    {
                        var employee = db.EmployeeBkps.FirstOrDefault(e => e.PayrollNo == approval.PayrollNo);
                        return new ApprovalViewModel
                        {
                            Approval = approval,
                            EmployeeName = employee != null ? employee.Fullname : string.Empty
                        };
                    }).ToList(),
                    Documents = documents.Select(document =>
                    {
                        var documentType = _context.DocumentType.FirstOrDefault(dt => dt.Id == document.DocumentTypeId);
                        return new DocumentViewModel
                        {
                            Document = document,
                            DocumentType = documentType
                        };
                    }).ToList(),
                    DepartmentName = department != null ? department.DepartmentName : string.Empty,
                    AddedBy = addedBy != null ? addedBy.Fullname : string.Empty,
                };


                //// VIEW MODEL FOR THE DYNAMIC TABS
                var tabViewModel = new TabViewModel
                {
                    Tabs = new List<TabViewModel.TabItem>
        {
            new TabViewModel.TabItem
            {
                Id = "tab-1",
                Title = "1. View Summary",
                PartialViewName = "_RequisitionSummary",
                Model = requisitionDetails,
                IsActive = true
            },
            new TabViewModel.TabItem
            {
                Id = "tab-2",
                Title = "2. Intern Details",
                PartialViewName = "_RequisitionIntern",
                Model = requisitionDetails,
                IsActive = false
            },
            new TabViewModel.TabItem
            {
                Id = "tab-3",
                Title = "3. Review Documents",
                PartialViewName = "_RequisitionDocument",
                Model = requisitionDetails,
                IsActive = false
            },
            new TabViewModel.TabItem
            {
                Id = "tab-4",
                Title = "4. Make Approval",
                PartialViewName = "_RequisitionApproval",
                Model = requisitionDetails,
                IsActive = false
            }
        }
                };
                var viewModel = new DetailsViewModel
                {
                    RequisitionDetails = requisitionDetails,
                    TabViewModel = tabViewModel
                };

                return View(viewModel);
            }
        }

        // GET: Requisitions/Create
        public IActionResult Create()
        {
            using (var db = new KtdaleaveContext())
            {
                var departments = db.Departments.ToList();

                // Create a SelectList from the departments
                var departmentItems = new SelectList(departments, "DepartmentCode", "DepartmentName");

                // Pass the SelectList to ViewBag
                ViewBag.DepartmentItems = departmentItems;
            }
            return View();
        }

        // POST: Requisitions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DepartmentCode,Intern_id,AddedBy,Station,Status,Start_Date,End_Date,CreatedAt,UpdatedAt")] Requisition requisition)
        {
            if (ModelState.IsValid)
            {
                string? PayrollNo = HttpContext.Session.GetString("EmployeePayrollNo");
                requisition.PayrollNo = PayrollNo;
                requisition.Status = "Active";
                requisition.CreatedAt = DateTime.Now;
                requisition.UpdatedAt = DateTime.Now;
                _context.Add(requisition);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(requisition);
        }

        // GET: Requisitions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requisition = await _relationshipService.GetRequisitionWithRelatedDataAsync((int)id);
            if (requisition == null)
            {
                return NotFound();
            }
            return View(requisition);
        }

        // POST: Requisitions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Department_id,Intern_id,AddedBy,Station,Status,Start_Date,End_Date,CreatedAt,UpdatedAt")] Requisition requisition)
        {
            if (id != requisition.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(requisition);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RequisitionExists(requisition.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(requisition);
        }

        // GET: Requisitions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var requisition = await _context.Requisition
                .FirstOrDefaultAsync(m => m.Id == id);
            if (requisition == null)
            {
                return NotFound();
            }

            return View(requisition);
        }

        // POST: Requisitions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var requisition = await _context.Requisition.FindAsync(id);
            if (requisition != null)
            {
                _context.Requisition.Remove(requisition);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RequisitionExists(int id)
        {
            return _context.Requisition.Any(e => e.Id == id);
        }
    }
}
