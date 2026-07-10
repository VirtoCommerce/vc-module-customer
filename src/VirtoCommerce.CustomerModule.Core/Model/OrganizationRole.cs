using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Core.Model;

public class OrganizationRole : Entity, ICloneable
{
    public string OrganizationId { get; set; }

    public string RoleId { get; set; }

    public string RoleName { get; set; }

    public virtual object Clone()
    {
        return MemberwiseClone();
    }
}
