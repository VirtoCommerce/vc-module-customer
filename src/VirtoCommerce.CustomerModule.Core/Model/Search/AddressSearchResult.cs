using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CustomerModule.Core.Model.Search;

public class AddressSearchResult : GenericSearchResult<Address>
{
    public AddressFacetResult Facets { get; set; } = new();
}

public class AddressFacetItem
{
    public string Value { get; set; }
    public int Count { get; set; }
    public bool IsApplied { get; set; }
    public string CountryCode { get; set; }
    public string RegionId { get; set; }
    public string Label { get; set; }
}

public class AddressFacetResult
{
    public Aggregation Country { get; set; }

    public Aggregation Region { get; set; }

    public Aggregation City { get; set; }

    public IList<AddressFacetItem> Countries { get; set; }

    public IList<AddressFacetItem> Regions { get; set; }

    public IList<AddressFacetItem> Cities { get; set; }

}
