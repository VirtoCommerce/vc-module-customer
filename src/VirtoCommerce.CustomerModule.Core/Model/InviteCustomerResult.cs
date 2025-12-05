using System.Collections.Generic;
using VirtoCommerce.Platform.Security;

namespace VirtoCommerce.CustomerModule.Core.Model;

public class InviteCustomerResult
{
    public bool Succeeded => Errors == null || Errors.Count == 0;
    public IList<CustomIdentityError> Errors { get; set; }
}
