using System.Collections.Generic;

namespace VirtoCommerce.CustomerModule.Core.Model;

public class InviteCustomerResult
{
    public bool Succeeded => Errors == null || Errors.Count == 0;

    public IList<InviteCustomerError> Errors { get; set; }
}
