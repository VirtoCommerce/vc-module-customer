using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerModule.Web.Authorization
{
    public sealed class AssociatedOrganizationsOnlyScope: PermissionScope
    {
        public AssociatedOrganizationsOnlyScope()
        {
            Scope = "{{userId}}";
        }
    }
}
