using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.Model;

public class OrganizationRoleEntity : Entity
{
    [Required]
    [StringLength(128)]
    public string OrganizationId { get; set; }

    [StringLength(128)]
    public string RoleId { get; set; }

    [StringLength(256)]
    public string RoleName { get; set; }

    public virtual OrganizationEntity Organization { get; set; }

    public virtual OrganizationRole ToModel(OrganizationRole model)
    {
        model.Id = Id;
        model.OrganizationId = OrganizationId;
        model.RoleId = RoleId;
        model.RoleName = RoleName;

        return model;
    }

    public virtual OrganizationRoleEntity FromModel(OrganizationRole model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.IsTransient() ? Guid.NewGuid().ToString("N") : model.Id;
        OrganizationId = model.OrganizationId;
        RoleId = model.RoleId;
        RoleName = model.RoleName;

        return this;
    }

    public virtual void Patch(OrganizationRoleEntity target)
    {
        target.RoleId = RoleId;
        target.RoleName = RoleName;
    }
}
