using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Workflows.Attributes
{
    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var isAuthenticated = context.HttpContext.Session.GetString("EmployeePayrollNo") != null;

            if (!isAuthenticated)
            {
                // Redirect to Login if not authenticated
                context.Result = new RedirectToActionResult("Index", "Login", null);
            }
        }
    }
}
