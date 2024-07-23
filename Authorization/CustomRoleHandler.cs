using Microsoft.AspNetCore.Authorization;

namespace Workflows.Authorization
{
    public class CustomRoleHandler : AuthorizationHandler<CustomRoleRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomRoleHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomRoleRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userRole = httpContext.Session.GetString("EmployeeRole");

            if (requirement.AllowedRoles.Contains(userRole))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
