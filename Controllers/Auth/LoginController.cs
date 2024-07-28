using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workflows.Extensions;
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
        [HttpGet]
        public ActionResult Index(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View("~/Views/Auth/Login.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string payrollNo, string password, string returnUrl = null)
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
                    // Store the whole employee object in session
                    HttpContext.Session.SetObject("Employee", employee);
                    HttpContext.Session.SetString("EmployeeName", employee.Fullname);
                    HttpContext.Session.SetString("EmployeePayrollNo", employee.PayrollNo);
                    HttpContext.Session.SetString("EmployeeDepartmentID", employee.Department);
                    HttpContext.Session.SetString("EmployeeRole", employee.Role);


                }
                Console.WriteLine($"ReturnUrl: {returnUrl}"); // Log the returnUrl
                Console.WriteLine($"Is Local URL: {Url.IsLocalUrl(returnUrl)}");
                // Authentication successful
                // Redirect to authenticated page
                if (!string.IsNullOrEmpty(returnUrl) 
                 //   && Url.IsLocalUrl(returnUrl)
                    )
                {
                    // Decode the URL before redirecting
                    returnUrl = Uri.UnescapeDataString(returnUrl);
                    return Redirect(returnUrl);
                }
                else
                {
                    Console.WriteLine("Redirecting to Requisitions Index");
                    return RedirectToAction("Index", "Requisitions");
                }
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
