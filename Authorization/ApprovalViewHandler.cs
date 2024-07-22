using Microsoft.AspNetCore.Authorization;
using Workflows.Models;

namespace Workflows.Authorization
{
    public class ApprovalViewHandler : AuthorizationHandler<ApprovalViewRequirement, Approval>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApprovalViewHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ApprovalViewRequirement requirement,
            Approval approval)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userRole = httpContext.Session.GetString("EmployeeRole");
            var userPayroll = httpContext.Session.GetString("EmployeePayrollNo");

            if (userRole == "Admin" || userRole == "HR")
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (approval.PayrollNo == userPayroll)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
