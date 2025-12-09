using System;
using System.Collections.Generic;

namespace VirtoCommerce.CustomerModule.Core.Model;

public class InviteCustomerRequest
{
    public string StoreId { get; set; }
    public string CultureName { get; set; }
    public string OrganizationId { get; set; }
    public string[] Emails { get; set; }
    public string Message { get; set; }
    public string[] RoleIds { get; set; }
    public string UrlSuffix { get; set; }
    public IDictionary<string, string> AdditionalParameters { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
