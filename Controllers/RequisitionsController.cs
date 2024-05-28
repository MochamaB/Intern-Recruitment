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
            var requisitions = await _context.Requisition.ToListAsync();
            // Get the list of departments from the KtdaleaveContext
            List<Department> departments;
            using (var ktdaContext = new KtdaleaveContext())
            {
                departments = await ktdaContext.Departments.ToListAsync();
            }

            // Pass both sets of data to the view
            ViewBag.Departments = departments;
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
