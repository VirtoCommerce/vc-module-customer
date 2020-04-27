using VirtoCommerce.Platform.Security.Authorization;

namespace VirtoCommerce.CustomerModule.Web.Authorization
{
    public sealed class CustomerAuthorizationRequirement : PermissionAuthorizationRequirement
    {
        public CustomerAuthorizationRequirement(string permission)
            : base(permission)
        {
        }
    }
}
