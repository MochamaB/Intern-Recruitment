using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Workflows.Attributes;
using Workflows.Data;
using Workflows.Models;
using Workflows.Services;

namespace Workflows.Controllers
{
    [CustomAuthorize] /// Used to ensure authenticated users view this class/pages
    public class ApprovalsController : Controller
    {
        private readonly WorkflowsContext _context;
        private readonly IEmailService _emailService;

        public ApprovalsController(WorkflowsContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: Approvals
        public async Task<IActionResult> Index()
        {
            var approvals = await _context.Approval
                .OrderByDescending(a => a.Requisition_id)
                .ToListAsync();

            // Get the list of departments from the KtdaleaveContext
            List<Department> departments;
            var employeeName = new Dictionary<string, string>();
            var internName = new Dictionary<int, string>();
            using (var ktdaContext = new KtdaleaveContext())
            {
                departments = await ktdaContext.Departments.ToListAsync();
               
                foreach (var approvalStep in approvals)
                {
                    var employee = ktdaContext.EmployeeBkps.FirstOrDefault(d => d.PayrollNo == approvalStep.PayrollNo);
                    var requisition = _context.Requisition.FirstOrDefault(d => d.Id == approvalStep.Requisition_id);
                    var intern = _context.Intern.FirstOrDefault(d => d.Id == requisition.Intern_id);
                    if (employee !=null && !employeeName.ContainsKey(employee.PayrollNo))
                    {
                        employeeName[employee.PayrollNo] = employee.Fullname;
                    }
                    // Add to the intern dictionary
                    if (intern != null && !internName.ContainsKey(requisition.Id))
                    {
                        internName[requisition.Id] = intern.Firstname + " " + intern.Lastname;
                    }
                }

            }

            // Pass both sets of data to the view
            ViewBag.EmployeeNames = employeeName;
            ViewBag.InternNames = internName;
            ViewBag.Departments = departments;
            return View(approvals);
        }

        // GET: Approvals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var approval = await _context.Approval
                .FirstOrDefaultAsync(m => m.Id == id);
            if (approval == null)
            {
                return NotFound();
            }

            return View(approval);
        }

        // GET: Approvals/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Approvals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Requisition_id,DepartmentCode,StepNumber,ApprovalStep,PayrollNo,ApprovalStatus,ApprovalComment,CreatedAt,UpdatedAt")] Approval approval)
        {
            if (ModelState.IsValid)
            {
                _context.Add(approval);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(approval);
        }

        // GET: Approvals/Edit/5
        public async Task<IActionResult> Edit(int? id, string returnUrl)
        {
            if (id == null)
            {
                return NotFound();
            }

            var approval = await _context.Approval.FindAsync(id);
            if (approval == null)
            {
                return NotFound();
            }
            if (approval.ApprovalStatus == "Cancelled")
            {
                // Return an error message to the view
                TempData["ErrorMessage"] = "This approval has been cancelled and cannot be changed.";
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine($"Department Code: {approval.DepartmentCode}");
            // Get the list of departments from the KtdaleaveContext
            List<EmployeeBkp> employees = new List<EmployeeBkp>(); // Initialize employees to an empty list
                                                                   // HR Department Code. To be changed later to get it through name and not hardcoded.
            int HRDepartmentCode = 104;
            string HRdepartmentCodeString = HRDepartmentCode.ToString();

            int approvalDepartmentCode = approval.DepartmentCode;
            string approvalDepartmentCodeString = approvalDepartmentCode.ToString();

            using (var db = new KtdaleaveContext())
            {

                if (approval.ApprovalStep == "HOD Approval")
                {
                    var department = await db.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == approval.DepartmentCode);
                    // Fetch employees in the same department as the approval
                    employees = await db.EmployeeBkps.Where(e => e.Department == department.DepartmentId &&
                    e.EmpisCurrActive == 0).OrderBy(e => e.Fullname).ToListAsync();
                }
                else if (approval.ApprovalStep == "HR Officer Approval" || approval.ApprovalStep == "HOH Approval")
                {
                    // Fetch employees in the HUMAN RESOURCE department
                    employees = await db.EmployeeBkps.Where(e => e.Department == HRdepartmentCodeString &&
                    e.EmpisCurrActive == 0).OrderBy(e => e.Fullname).ToListAsync();
                }

                if (!string.IsNullOrEmpty(returnUrl))
                {
                    TempData["ReturnUrl"] = returnUrl;
                }


                ViewBag.Employees = new SelectList(employees, "PayrollNo", "Fullname", approval.PayrollNo);
                return View(approval);
            }
        }

        // POST: Approvals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Requisition_id,DepartmentCode,StepNumber,ApprovalStep,PayrollNo,ApprovalStatus,ApprovalComment,CreatedAt,UpdatedAt")] Approval approval)
        {
            if (id != approval.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    approval.UpdatedAt = DateTime.Now;
                    _context.Update(approval);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApprovalExists(approval.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Approval Edited successfully!";
                // Returns to the where the edit function was called
                if (TempData.TryGetValue("ReturnUrl", out object returnUrl))
                {
                    return Redirect(returnUrl.ToString());
                }

                return RedirectToAction(nameof(Index));
            }
           
            return View(approval);
        }

        public async Task<IActionResult> MakeApproval(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var approval = await _context.Approval.FindAsync(id);
            if (approval == null)
            {
                return NotFound();
            }
            return View(approval);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeApproval(int id, [Bind("Id,Requisition_id,DepartmentCode,StepNumber,ApprovalStep,PayrollNo,ApprovalStatus,ApprovalComment,CreatedAt,UpdatedAt")] Approval approval)
        {
            if (id != approval.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    approval.UpdatedAt = DateTime.Now;
                    _context.Update(approval);
                    await _context.SaveChangesAsync();

                    // Call the method or service to handle the approval flow
                    await HandleApprovalFlow(approval);

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApprovalExists(approval.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Approval Made successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(approval);
        }


        private async Task HandleApprovalFlow(Approval currentApproval)
        {
            //1. Send Email to the employee that made the requisition
            //2. Get the next approval in the sequence
            //3. Update the next approval status to "Pending"
            //4. Send email notification for approval pending to the next approver
            //5. if it is the last approval, update the requisition status to "Active"
            //6. If it is the last approval. Update the intern status to "Active"
            //7. If the approval is rejected. send email to the employee who requested rejecting the requisition
            var currentRequisition = await _context.Requisition.FindAsync(currentApproval.Requisition_id);
            if (currentApproval.ApprovalStatus == "Approved")
            {
                // 1.Send email notification to employee that made the requisition
                     //   var currentRequisition = await _context.Requisition.FindAsync(currentApproval.Requisition_id);
                if (currentRequisition != null)
                {
                    using (var db = new KtdaleaveContext())
                    {
                        //   var employee = await db.EmployeeBkps.FindAsync(requisition.PayrollNo);
                        var employee =  db.EmployeeBkps.FirstOrDefault(d => d.PayrollNo == currentRequisition.PayrollNo);
                        var currentApprover = db.EmployeeBkps.FirstOrDefault(d => d.PayrollNo == currentApproval.PayrollNo);

                        var requesterEmail = employee.EmailAddress;
                        var currentApproverName = currentApprover.Fullname;
                        int stepNumber = currentApproval.StepNumber; // Assuming this is an integer
                        string approvalStep = currentApproval.ApprovalStep; // Assuming this is a string
                        // Concatenate step number and approval step
                        string currentStep = stepNumber + ". " + approvalStep;
                        await _emailService.SendApprovalMadeNotificationAsync(requesterEmail, currentApproverName, currentStep, currentRequisition.Id);
                    }
                }

                // 2. Get the next approval in the flow
                var nextApproval = await _context.Approval
                    .Where(a => a.Requisition_id == currentApproval.Requisition_id && a.StepNumber == currentApproval.StepNumber + 1)
                    .FirstOrDefaultAsync();

                if (nextApproval != null)
                {
                // 3.Update the next approval status to "Pending"
                    nextApproval.ApprovalStatus = "Pending";
                    nextApproval.UpdatedAt = DateTime.Now;
                    _context.Update(nextApproval);
                    await _context.SaveChangesAsync();

                // 4.Send email notification for approval pending
                    using (var db = new KtdaleaveContext())
                    {
                     //   var approver = await db.EmployeeBkps.FindAsync(nextApproval.PayrollNo);
                        var approver = db.EmployeeBkps.FirstOrDefault(d => d.PayrollNo == nextApproval.PayrollNo);
                        if (approver != null)
                        {
                            await _emailService.SendApprovalPendingNotificationAsync(approver.EmailAddress, nextApproval.Requisition_id);
                        }
                    }
                }
                else
                {
                // 5.This is the last approval, update the requisition status to "Active"
                    var requisition = await _context.Requisition.FindAsync(currentApproval.Requisition_id);
                    if (requisition != null)
                    {
                        requisition.Status = "Active";
                        requisition.UpdatedAt = DateTime.Now;
                        _context.Update(requisition);
                 // 6.Update the intern status to "Active"
                        var intern = await _context.Intern.FindAsync(requisition.Intern_id);
                        if (intern != null)
                        {
                            intern.Status = "Active";
                            intern.UpdatedAt = DateTime.Now;  
                            _context.Update(intern);
                        }
                    }
                    
                }
                await _context.SaveChangesAsync();
            }
            else if (currentApproval.ApprovalStatus == "Rejected")
            {
                // 7. send email to the employee who requested rejecting the requisition
                if (currentRequisition != null)
                {

                    using (var db = new KtdaleaveContext())
                    {
                        //   var employee = await db.EmployeeBkps.FindAsync(requisition.PayrollNo);
                        var employee = db.EmployeeBkps.FirstOrDefault(d => d.PayrollNo == currentRequisition.PayrollNo);

                        var requesterEmail = employee.EmailAddress;
                        await _emailService.SendApprovalRejectedNotificationAsync(requesterEmail, currentRequisition.Id);
                    }
                }

                // 8. Cancel all subsequent approvals
                var subsequentApprovals = await _context.Approval
                    .Where(a => a.Requisition_id == currentApproval.Requisition_id && a.StepNumber > currentApproval.StepNumber)
                    .ToListAsync();

                foreach (var approval in subsequentApprovals)
                {
                    approval.ApprovalStatus = "Cancelled";
                    approval.UpdatedAt = DateTime.Now;
                    _context.Update(approval);
                }

                await _context.SaveChangesAsync();
            }
        }


        // GET: Approvals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var approval = await _context.Approval
                .FirstOrDefaultAsync(m => m.Id == id);
            if (approval == null)
            {
                return NotFound();
            }

            return View(approval);
        }

        // POST: Approvals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var approval = await _context.Approval.FindAsync(id);
            if (approval != null)
            {
                _context.Approval.Remove(approval);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ApprovalExists(int id)
        {
            return _context.Approval.Any(e => e.Id == id);
        }
    }
}
