using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerModule.Core.Services;

public static class CompanyMemberRoleWhitelistExtensions
{
    public static bool IsRoleAllowed(this IReadOnlyCollection<string> allowedRoleIds, Role role)
    {
        return allowedRoleIds.Contains(role.Id, StringComparer.OrdinalIgnoreCase) ||
               allowedRoleIds.Contains(role.Name, StringComparer.OrdinalIgnoreCase);
    }
}
