using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Workflows.Authorization;

namespace Workflows.Services
{
    public class CustomAuthorizationService : IAuthorizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationHandler _handler;

        public CustomAuthorizationService(IHttpContextAccessor httpContextAccessor, IAuthorizationHandler handler)
        {
            _httpContextAccessor = httpContextAccessor;
            _handler = handler;
        }

        public async Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource, IEnumerable<IAuthorizationRequirement> requirements)
        {
            var authContext = new AuthorizationHandlerContext(requirements, user, resource);
            await _handler.HandleAsync(authContext);
            return authContext.HasSucceeded ? AuthorizationResult.Success() : AuthorizationResult.Failed();
        }

        public async Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource, string policyName)
        {
            var requirement = new CustomRoleRequirement(policyName.Split(','));
            return await AuthorizeAsync(user, resource, new[] { requirement });
        }

       
    }
}
