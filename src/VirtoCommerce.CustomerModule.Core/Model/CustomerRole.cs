using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Core.Model;

public class CustomerRole : Entity
{
    public string Name { get; set; }

    public string Description { get; set; }
}

public class CustomerRoleSearchResult : GenericSearchResult<CustomerRole>
{
}
