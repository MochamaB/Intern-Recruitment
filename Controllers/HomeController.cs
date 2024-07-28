using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using Workflows.Data;
using Workflows.Models;
using Workflows.ViewModels;

namespace Workflows.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly WorkflowsContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(ILogger<HomeController> logger, WorkflowsContext context, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userRole = httpContext.Session.GetString("EmployeeRole");
            var userPayroll = httpContext.Session.GetString("EmployeePayrollNo");

            //// FILTER: Loggedin User can only see approvals assigned to them unless HR or Admin Roles
            
            var approvalCount = _context.Approval
              .Where(a => (userRole == "Admin" || userRole == "HR" || a.PayrollNo == userPayroll) && a.ApprovalStatus != "Approved" )
              .Count();
            ViewBag.ApprovalCount = approvalCount;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public async Task<IActionResult> SidebarMenu()
        {
            var userPayroll = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var approvalCount = await _context.Approval
                .Where(a => (userRole == "Admin" || userRole == "HR" || a.PayrollNo == userPayroll) && a.ApprovalStatus != "Approved")
                .CountAsync();
            Console.WriteLine($"approvalCount: {approvalCount}");
            ViewBag.ApprovalCount = approvalCount;

            var model = new SidebarViewModel
            {
                ApprovalCount = approvalCount
            };

            return PartialView("_Sidebar",model);
        }
    }
}
