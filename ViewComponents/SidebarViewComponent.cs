using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Workflows.Data;
using Workflows.ViewModels;

namespace Workflows.ViewComponents
{
    public class SidebarViewComponent : ViewComponent
    {
        private readonly WorkflowsContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SidebarViewComponent(WorkflowsContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userRole = httpContext.Session.GetString("EmployeeRole");
            var userPayroll = httpContext.Session.GetString("EmployeePayrollNo");

            var approvalCount = await _context.Approval
                .Where(a => (userRole == "Admin" || userRole == "HR" || a.PayrollNo == userPayroll) && a.ApprovalStatus != "Approved")
                .CountAsync();

            var model = new SidebarViewModel
            {
                ApprovalCount = approvalCount
            };

            return View(model);
        }
    }
}
