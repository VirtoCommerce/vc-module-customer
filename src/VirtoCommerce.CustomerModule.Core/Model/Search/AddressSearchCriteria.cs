using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Core.Model.Search;

public class AddressSearchCriteria : SearchCriteriaBase
{
    public string MemberId { get; set; }

    public IList<string> CountryCodes { get; set; }

    public IList<string> RegionIds { get; set; }

    public IList<string> Cities { get; set; }
}
