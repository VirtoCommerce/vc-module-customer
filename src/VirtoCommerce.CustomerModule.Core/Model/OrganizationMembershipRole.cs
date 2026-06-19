using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Core.Model;

public class OrganizationMembershipRole : Entity, ICloneable
{
    public string MembershipId { get; set; }

    public string RoleId { get; set; }

    public string RoleName { get; set; }

    public virtual object Clone()
    {
        return MemberwiseClone();
    }
}
