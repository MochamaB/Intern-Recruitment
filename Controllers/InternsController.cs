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
using Workflows.ViewModels;

namespace Workflows.Controllers
{
    [CustomAuthorize] /// Used to ensure authenticated users view this class/pages
    public class InternsController : Controller
    {
        private readonly WorkflowsContext _context;
        private readonly KtdaleaveContext _ktdaleavecontext;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public InternsController(WorkflowsContext context, KtdaleaveContext ktdaleavecontext, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _ktdaleavecontext = ktdaleavecontext ?? throw new ArgumentNullException(nameof(ktdaleavecontext));
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Interns
        public async Task<IActionResult> Index(InternFilterViewModel filter)
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
            // FILTERS
            var query = _context.Intern
            .Where(r => userRole == "Admin" || userRole == "HR" || r.DepartmentCode == departmentCode);

            if (filter.DepartmentCode.HasValue)
            {
                query = query.Where(r => r.DepartmentCode == filter.DepartmentCode.Value);
            }
            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(r => r.Status == filter.Status);
            }
            if (!string.IsNullOrEmpty(filter.School))
            {
                query = query.Where(r => r.School == filter.School);
            }
            // Get the list of interns from the WorkflowContext
            var interns = await query
             .OrderByDescending(e => e.Id)
             .Take(50)
             .ToListAsync();

            // Get the list of departments from the KtdaleaveContext
            List<Department> departments;
          
                departments = await ktdaContext.Departments.ToListAsync();
            

            // Pass both sets of data to the view
            ViewBag.Departments = departments;
            var viewModel = new InternIndexViewModel
            {
                Filter = filter,
                Interns = interns
            };
            viewModel.Filter.DepartmentList = await ktdaContext.Departments
            .Select(d => new SelectListItem { Value = d.DepartmentCode.ToString(), Text = d.DepartmentName })
            .ToListAsync();

            viewModel.Filter.StatusList = await _context.Intern
              .Select(r => r.Status)
              .Distinct()
              .Select(s => new SelectListItem { Value = s, Text = s })
              .ToListAsync();

            viewModel.Filter.SchoolList = await _context.Intern
            .Select(r => r.School)
            .Distinct()
            .Select(s => new SelectListItem { Value = s, Text = s })
            .ToListAsync();

            return View(viewModel);
        }

        // GET: Interns/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var intern = await _context.Intern
                .FirstOrDefaultAsync(m => m.Id == id);
            if (intern == null)
            {
                return NotFound();
            }

            return View(intern);
        }

        // GET: Interns/Create
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

        //    ViewBag.Department_id = new SelectList(departments, "DepartmentID", "DepartmentName");
            return View();
        }

        // POST: Interns/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DepartmentCode,Idnumber,Firstname,Lastname,Othernames,Email,PhoneNumber,Status,Certification,Course,School,CreatedAt,UpdatedAt")] Intern intern)
        {
            if (ModelState.IsValid)
            {
                intern.Status =   "Inactive";
                intern.CreatedAt = DateTime.Now;
                intern.UpdatedAt = DateTime.Now;
                _context.Add(intern);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(intern);
        }

        // GET: Interns/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, string returnUrl)
        {
            if (id == null)
            {
                return NotFound();
            }

            var intern = await _context.Intern.FindAsync(id);
            if (intern == null)
            {
                return NotFound();
            }
            /// Gets the url returned from edit link.
            if (!string.IsNullOrEmpty(returnUrl))
            {
                TempData["ReturnUrl"] = returnUrl;
            }
            return View(intern);
        }

        // POST: Interns/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DepartmentCode,Idnumber,Firstname,Lastname,Othernames,Email,PhoneNumber,Status,Certification,Course,School,CreatedAt,UpdatedAt")] Intern intern)
        {
            if (id != intern.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    intern.UpdatedAt = DateTime.Now;
                    _context.Update(intern);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InternExists(intern.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Returns to the where the edit function was called
                if (TempData.TryGetValue("ReturnUrl", out object returnUrl))
                {
                    return Redirect(returnUrl.ToString());
                }
                return RedirectToAction(nameof(Index));
            }
            return View(intern);
        }

        // GET: Interns/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var intern = await _context.Intern
                .FirstOrDefaultAsync(m => m.Id == id);
            if (intern == null)
            {
                return NotFound();
            }

            return View(intern);
        }

        // POST: Interns/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var intern = await _context.Intern.FindAsync(id);
            if (intern != null)
            {
                _context.Intern.Remove(intern);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InternExists(int id)
        {
            return _context.Intern.Any(e => e.Id == id);
        }
    }
}
