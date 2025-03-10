using System.Collections.Generic;

namespace VirtoCommerce.CustomerModule.Core.Model;

public interface IHasOrganizations
{
    IList<string> Organizations { get; set; }
    string DefaultOrganizationId { get; set; }
    string CurrentOrganizationId { get; set; }
}
