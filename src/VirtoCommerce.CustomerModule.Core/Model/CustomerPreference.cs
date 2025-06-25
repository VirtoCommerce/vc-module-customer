using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Core.Model;

public class CustomerPreference : AuditableEntity, ICloneable
{
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
