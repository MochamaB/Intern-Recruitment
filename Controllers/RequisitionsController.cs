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

namespace Workflows.Controllers
{
    [CustomAuthorize] /// Used to ensure authenticated users view this class/pages
    public class RequisitionsController : Controller
    {
        private readonly WorkflowsContext _context;

        public RequisitionsController(WorkflowsContext context)
        {
            _context = context;
        }

        // GET: Requisitions
        public async Task<IActionResult> Index()
        {
            var requisitions = await _context.Requisition.OrderByDescending(e => e.Id).ToListAsync();
            // Get the list of departments from the KtdaleaveContext
            var employeeName = new Dictionary<string, string>();
            var internName = new Dictionary<int, string>();
            List<Department> departments;
            var approvalStatuses = new Dictionary<int, string>(); // RequisitionId to ApprovalStatus with pending
            var approvalEmployeeNames = new Dictionary<int, string>(); // RequisitionId to Approval EmployeeName
            var StepNumber = new Dictionary<int, int>();
            var approvalStep = new Dictionary<int, string>();
            var approvalComment = new Dictionary<int, string>();
            var daysLeftDictionary = new Dictionary<int, int>();
            using (var ktdaContext = new KtdaleaveContext())
            {
                departments = await ktdaContext.Departments.ToListAsync();
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
                    // Get the remaining Days
                    TimeSpan difference = requisition.End_Date - requisition.Start_Date;
                    daysLeftDictionary[requisition.Id] = (int)difference.TotalDays;

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
            return View(requisitions);
        }

        // GET: Requisitions/Details/5
        public async Task<IActionResult> Details(int? id)
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

            var requisition = await _context.Requisition.FindAsync(id);
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
