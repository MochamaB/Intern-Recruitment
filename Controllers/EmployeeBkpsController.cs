using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Workflows.Attributes;
using Workflows.Models;
using Workflows.ViewModels;

namespace Workflows.Controllers
{
    [CustomAuthorize] /// Used to ensure authenticated users view this class/pages
    public class EmployeeBkpsController : Controller
    {
        private readonly KtdaleaveContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmployeeBkpsController(KtdaleaveContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: EmployeeBkps
        public async Task<IActionResult> Index(EmployeeFilterViewModel filter)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userRole = httpContext.Session.GetString("EmployeeRole");
            var userPayroll = httpContext.Session.GetString("EmployeePayrollNo");
            var sessionDepartmentID = httpContext.Session.GetString("EmployeeDepartmentID");

            var query = _context.EmployeeBkps
           .Where(r => userRole == "Admin" || userRole == "HR" || r.Department == sessionDepartmentID);

            if (!string.IsNullOrEmpty(filter.Department))
            {
                query = query.Where(r => r.Department == filter.Department);
            }
            if (!string.IsNullOrEmpty(filter.Scale))
            {
                query = query.Where(r => r.Scale == filter.Scale);
            }
            if (!string.IsNullOrEmpty(filter.Role))
            {
                query = query.Where(r => r.Role == filter.Role);
            }

            var employees = await query
           .OrderByDescending(e => e.LastPay)
           .Take(100)
           .ToListAsync();

            var departments = await _context.Departments.GroupBy(d => d.DepartmentId).ToDictionaryAsync(g => g.Key, g => g.First().DepartmentName);
           
            var employeeViewModels = employees.Select(e => new EmployeeViewModel
            {
                Fullname = e.Fullname,
                Username = e.Username,
                Designation = e.Designation,
                EmailAddress = e.EmailAddress,
                PayrollNo = e.PayrollNo,
                Station = e.Station,
                RoleName = e.Role,
                Scale = e.Scale,
                EmployeId = e.EmployeId,
                ContractEnd = e.ContractEnd,
                HireDate = e.HireDate,
                LastPay = e.LastPay,
                EmpisCurrActive = e.EmpisCurrActive,
                Hod = e.Hod,
                Supervisor = e.Supervisor,
                DepartmentName = e.Department != null && departments.TryGetValue(e.Department, out var departmentName) ? departmentName : null
            }).ToList();

            var viewModel = new EmployeeIndexViewModel
            {
                Filter = filter,
                Employees = employeeViewModels
            };

            viewModel.Filter.DepartmentList = await _context.Departments
           .Select(d => new SelectListItem { Value = d.DepartmentCode.ToString(), Text = d.DepartmentName })
           .ToListAsync();

            viewModel.Filter.ScaleList = await _context.EmployeeBkps
              .Select(r => r.Scale)
              .Distinct()
              .Select(s => new SelectListItem { Value = s, Text = s })
              .ToListAsync();

            viewModel.Filter.RoleList = await _context.EmployeeBkps
            .Select(r => r.Role)
            .Distinct()
            .Select(s => new SelectListItem { Value = s, Text = s })
            .ToListAsync();

            return View(viewModel);
        }

        // GET: EmployeeBkps/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeBkp = await _context.EmployeeBkps
                .FirstOrDefaultAsync(m => m.PayrollNo == id);
            if (employeeBkp == null)
            {
                return NotFound();
            }

            return View(employeeBkp);
        }

        // GET: EmployeeBkps/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EmployeeBkps/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Designation,EmailAddress,SurName,OtherNames,PayrollNo,Station,PasswordP,Hod,Supervisor,Role,OtherName,Department,EmployeId,ContractEnd,HireDate,ServiceYears,Username,RetireDate,Pass,RollNo,LastPay,Scale,EmpisCurrActive,OrgGroup,Fullname")] EmployeeBkp employeeBkp)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employeeBkp);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employeeBkp);
        }

        // GET: EmployeeBkps/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeBkp = await _context.EmployeeBkps.FindAsync(id);
            if (employeeBkp == null)
            {
                return NotFound();
            }
            return View(employeeBkp);
        }

        // POST: EmployeeBkps/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Designation,EmailAddress,SurName,OtherNames,PayrollNo,Station,PasswordP,Hod,Supervisor,Role,OtherName,Department,EmployeId,ContractEnd,HireDate,ServiceYears,Username,RetireDate,Pass,RollNo,LastPay,Scale,EmpisCurrActive,OrgGroup,Fullname")] EmployeeBkp employeeBkp)
        {
            if (id != employeeBkp.PayrollNo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employeeBkp);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeBkpExists(employeeBkp.PayrollNo))
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
            return View(employeeBkp);
        }

        // GET: EmployeeBkps/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employeeBkp = await _context.EmployeeBkps
                .FirstOrDefaultAsync(m => m.PayrollNo == id);
            if (employeeBkp == null)
            {
                return NotFound();
            }

            return View(employeeBkp);
        }

        // POST: EmployeeBkps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var employeeBkp = await _context.EmployeeBkps.FindAsync(id);
            if (employeeBkp != null)
            {
                _context.EmployeeBkps.Remove(employeeBkp);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeBkpExists(string id)
        {
            return _context.EmployeeBkps.Any(e => e.PayrollNo == id);
        }
    }
}
