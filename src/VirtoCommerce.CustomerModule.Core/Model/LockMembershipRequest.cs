using System;

namespace VirtoCommerce.CustomerModule.Core.Model;

public class LockMembershipRequest
{
    public DateTime? LockoutEnd { get; set; }
}
