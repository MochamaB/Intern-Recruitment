using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workflows.Models;
using Workflows.Services;

namespace Workflows.Controllers.Auth
{
    public class LoginController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly KtdaleaveContext _context;
        public LoginController(IAuthenticationService authenticationService, KtdaleaveContext context)
        {
            _authenticationService = authenticationService;
            _context = context;
        }
        // GET: LoginController
        public ActionResult Index()
        {
            return View("~/Views/Auth/Login.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string payrollNo, string password)
        {
            // Check if payrollNo or password is null or empty
              if (string.IsNullOrEmpty(payrollNo) || string.IsNullOrEmpty(password))
               {
                    ViewBag.ErrorMessage = "Payroll number and password are required.";
                    return View("~/Views/Auth/Login.cshtml");
                }

            Console.WriteLine($"Received payrollNo: {payrollNo}, password: {password}");
            if (await _authenticationService.AuthenticateAsync(payrollNo, password))
            {
                // Fetch employee details directly using DbContext
                var employee = await _context.EmployeeBkps.SingleOrDefaultAsync(e => e.PayrollNo == payrollNo);

                if (employee != null)
                {
                    // Store employee information in session
                    HttpContext.Session.SetString("EmployeeName", employee.Fullname);
                    HttpContext.Session.SetString("EmployeePayrollNo", employee.PayrollNo);
                    HttpContext.Session.SetString("EmployeeDepartment", employee.Department);
                    HttpContext.Session.SetString("EmployeeRole", employee.Role);


                }
                    // Authentication successful
                    // Redirect to authenticated page
                    return RedirectToAction("Index", "Requisitions");
            }
            else
            {
                // Authentication failed
                // Return login view with error message
                ViewBag.ErrorMessage = "The payroll number and password do not match";
                return View("~/Views/Auth/Login.cshtml");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();

            // Redirect to the login page
            return View("~/Views/Auth/Login.cshtml");
        }

        // GET: LoginController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: LoginController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LoginController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LoginController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LoginController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LoginController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LoginController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
