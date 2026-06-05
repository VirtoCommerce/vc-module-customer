using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.CustomerModule.Data.Model;

public class OrganizationMembershipEntity : AuditableEntity, IDataEntity<OrganizationMembershipEntity, OrganizationMembership>
{
    [Required]
    [StringLength(128)]
    public string UserId { get; set; }

    [StringLength(128)]
    public string OrganizationId { get; set; }

    public bool IsLocked { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public virtual ObservableCollection<OrganizationMembershipRoleEntity> Roles { get; set; }
        = new NullCollection<OrganizationMembershipRoleEntity>();

    public virtual OrganizationMembership ToModel(OrganizationMembership model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.UserId = UserId;
        model.OrganizationId = OrganizationId;
        model.IsLocked = IsLocked;
        model.LockoutEnd = LockoutEnd;
        model.Roles = Roles.Select(r => r.ToModel(new OrganizationMembershipRole())).ToList();

        return model;
    }

    public virtual OrganizationMembershipEntity FromModel(OrganizationMembership model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.IsTransient() ? Guid.NewGuid().ToString("N") : model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        UserId = model.UserId;
        OrganizationId = model.OrganizationId;
        IsLocked = model.IsLocked;
        LockoutEnd = model.LockoutEnd;

        if (model.Roles != null)
        {
            Roles = new ObservableCollection<OrganizationMembershipRoleEntity>(
                model.Roles.Select(r =>
                {
                    var roleEntity = new OrganizationMembershipRoleEntity().FromModel(r, pkMap);
                    roleEntity.MembershipId = Id;
                    return roleEntity;
                }));
        }

        return this;
    }

    public virtual void Patch(OrganizationMembershipEntity target)
    {
        target.UserId = UserId;
        target.OrganizationId = OrganizationId;
        target.IsLocked = IsLocked;
        target.LockoutEnd = LockoutEnd;

        if (!Roles.IsNullCollection())
        {
            Roles.Patch(target.Roles, (src, dst) => src.Patch(dst));
        }
    }
}
