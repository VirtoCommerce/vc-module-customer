using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.Model;

public class OrganizationMembershipRoleEntity : Entity
{
    [Required]
    [StringLength(128)]
    public string MembershipId { get; set; }

    [StringLength(128)]
    public string RoleId { get; set; }

    [StringLength(256)]
    public string RoleName { get; set; }

    public virtual OrganizationMembershipEntity Membership { get; set; }

    public virtual OrganizationMembershipRole ToModel(OrganizationMembershipRole model)
    {
        model.Id = Id;
        model.MembershipId = MembershipId;
        model.RoleId = RoleId;
        model.RoleName = RoleName;

        return model;
    }

    public virtual OrganizationMembershipRoleEntity FromModel(OrganizationMembershipRole model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.Id;
        MembershipId = model.MembershipId;
        RoleId = model.RoleId;
        RoleName = model.RoleName;

        return this;
    }

    public virtual void Patch(OrganizationMembershipRoleEntity target)
    {
        target.RoleId = RoleId;
        target.RoleName = RoleName;
    }
}
