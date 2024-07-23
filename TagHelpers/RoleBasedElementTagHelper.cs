using Microsoft.AspNetCore.Razor.TagHelpers;
using Workflows.Services;

namespace Workflows.TagHelpers
{
    [HtmlTargetElement("*", Attributes = "asp-authorize-role")]
    public class RoleBasedElementTagHelper : TagHelper
    {
        private readonly CustomAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RoleBasedElementTagHelper(CustomAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HtmlAttributeName("asp-authorize-role")]
        public string Roles { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext.User;

            var authorized = await _authorizationService.AuthorizeAsync(user, null, Roles);

            if (!authorized.Succeeded)
            {
                output.SuppressOutput();
            }
        }
    }
}
