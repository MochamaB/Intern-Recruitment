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

namespace Workflows.Controllers
{
    [CustomAuthorize] /// Used to ensure authenticated users view this class/pages
    public class ApprovalsController : Controller
    {
        private readonly WorkflowsContext _context;

        public ApprovalsController(WorkflowsContext context)
        {
            _context = context;
        }

        // GET: Approvals
        public async Task<IActionResult> Index()
        {
            var approvals = await _context.Approval.ToListAsync();
            // Group the approvals by Requisition_id
            var groupedApprovals = approvals.GroupBy(a => a.Requisition_id);
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
        public async Task<IActionResult> Create([Bind("Id,Requisition_id,DepartmentCode,ApprovalStep,PayrollNo,ApprovalStatus,ApprovalComment,CreatedAt,UpdatedAt")] Approval approval)
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
        public async Task<IActionResult> Edit(int? id)
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
                    e.EmpisCurrActive == 0).ToListAsync();
                }
                else if (approval.ApprovalStep == "HR Officer Approval" || approval.ApprovalStep == "HOH Approval")
                {
                    // Fetch employees in the HUMAN RESOURCE department
                    employees = await db.EmployeeBkps.Where(e => e.Department == HRdepartmentCodeString &&
                    e.EmpisCurrActive == 0).ToListAsync();
                }


                ViewBag.Employees = employees;
                return View(approval);
            }
        }

        // POST: Approvals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Requisition_id,DepartmentCode,ApprovalStep,PayrollNo,ApprovalStatus,ApprovalComment,CreatedAt,UpdatedAt")] Approval approval)
        {
            if (id != approval.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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
        public async Task<IActionResult> MakeApproval(int id, [Bind("Id,Requisition_id,DepartmentCode,ApprovalStep,PayrollNo,ApprovalStatus,ApprovalComment,CreatedAt,UpdatedAt")] Approval approval)
        {
            if (id != approval.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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
                return RedirectToAction(nameof(Index));
            }
            return View(approval);
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
