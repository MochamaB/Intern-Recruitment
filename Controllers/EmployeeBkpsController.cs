using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Workflows.Models;

namespace Workflows.Controllers
{
    public class EmployeeBkpsController : Controller
    {
        private readonly KtdaleaveContext _context;

        public EmployeeBkpsController(KtdaleaveContext context)
        {
            _context = context;
        }

        // GET: EmployeeBkps
        public async Task<IActionResult> Index()
        {
            return View(await _context.EmployeeBkps.ToListAsync());
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
