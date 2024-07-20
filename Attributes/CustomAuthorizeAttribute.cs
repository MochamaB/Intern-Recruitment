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
                // Get the current request path and query string
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;

                // Encode the returnUrl to ensure it's properly formatted in the URL
                var encodedReturnUrl = Uri.EscapeDataString(returnUrl);

                // Redirect to Login if not authenticated
                context.Result = new RedirectToActionResult("Index", "Login", new { returnUrl = encodedReturnUrl });
            }
        }
    }
}
