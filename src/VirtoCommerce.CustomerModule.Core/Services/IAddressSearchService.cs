using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CustomerModule.Core.Services;

public interface IAddressSearchService : ISearchService<AddressSearchCriteria, AddressSearchResult, Address>;
