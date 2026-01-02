using System.Collections.Generic;

namespace VirtoCommerce.CustomerModule.Core.Model;

public class InviteCustomerResult
{
    public bool Succeeded { get; set; }

    public IList<InviteCustomerError> Errors { get; set; }
}
