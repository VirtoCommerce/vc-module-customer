using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.CustomerModule.Data.Model;

public class CustomerPreferenceEntity : AuditableEntity, IDataEntity<CustomerPreferenceEntity, CustomerPreference>
{
    [Required]
    public string UserId { get; set; }

    [Required]
    public string Name { get; set; }

    public string Value { get; set; }

    public virtual CustomerPreference ToModel(CustomerPreference model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.UserId = UserId;
        model.Name = Name;
        model.Value = Value;

        return model;
    }

    public virtual CustomerPreferenceEntity FromModel(CustomerPreference model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        UserId = model.UserId;
        Name = model.Name;
        Value = model.Value;

        return this;
    }

    public virtual void Patch(CustomerPreferenceEntity target)
    {
        target.UserId = UserId;
        target.Name = Name;
        target.Value = Value;
    }
}
