using System;
using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Core.Model;

public class OrganizationMembership : AuditableEntity, ICloneable
{
    public string UserId { get; set; }

    public string OrganizationId { get; set; }

    public string OrganizationName { get; set; }

    public bool IsLocked { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public bool IsCurrentlyLocked => IsLocked && (!LockoutEnd.HasValue || LockoutEnd.Value > DateTime.UtcNow);

    public IList<OrganizationMembershipRole> Roles { get; set; } = [];

    public virtual object Clone()
    {
        var clone = (OrganizationMembership)MemberwiseClone();

        clone.Roles = Roles?.Select(r => (OrganizationMembershipRole)r.Clone()).ToList() ?? [];

        return clone;
    }
}
