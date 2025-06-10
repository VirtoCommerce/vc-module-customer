using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CustomerModule.Core.Services;

public interface ICustomerPreferenceSearchService : ISearchService<CustomerPreferenceSearchCriteria, CustomerPreferenceSearchResult, CustomerPreference>;
