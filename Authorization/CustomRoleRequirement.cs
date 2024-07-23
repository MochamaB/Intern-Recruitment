using Microsoft.AspNetCore.Authorization;

namespace Workflows.Authorization
{
    public class CustomRoleRequirement : IAuthorizationRequirement
    {
        public string[] AllowedRoles { get; }

        public CustomRoleRequirement(params string[] roles)
        {
            AllowedRoles = roles;
        }
    }
}
