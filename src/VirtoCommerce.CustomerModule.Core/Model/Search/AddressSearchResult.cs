using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CustomerModule.Core.Model.Search;

public class AddressSearchResult : GenericSearchResult<Address>
{
    public AddressFacetResult Facets { get; set; } = new();
}

public class AddressFacetResult
{
    public Aggregation Country { get; set; }

    public Aggregation Region { get; set; }

    public Aggregation City { get; set; }
}
