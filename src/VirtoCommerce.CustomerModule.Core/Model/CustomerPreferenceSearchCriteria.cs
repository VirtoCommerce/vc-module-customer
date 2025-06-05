using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Core.Model;

public class CustomerPreferenceSearchCriteria : SearchCriteriaBase
{
    public string UserId { get; set; }
    public string Name { get; set; }
}
