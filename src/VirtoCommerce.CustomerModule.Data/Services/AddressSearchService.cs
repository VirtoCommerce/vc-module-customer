using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.CustomerModule.Data.Services;

public class AddressSearchService(
    Func<IMemberRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    IAddressService crudService,
    IOptions<CrudOptions> crudOptions)
    : SearchService<AddressSearchCriteria, AddressSearchResult, Address, AddressEntity>
        (repositoryFactory, platformMemoryCache, crudService, crudOptions),
        IAddressSearchService
{
    protected override IQueryable<AddressEntity> BuildQuery(IRepository repository, AddressSearchCriteria criteria)
    {
        var query = ((IMemberRepository)repository).Addresses;

        if (!criteria.Keyword.IsNullOrEmpty())
        {
            query = query.Where(x => x.Name.Contains(criteria.Keyword) ||
                x.Description.Contains(criteria.Keyword) ||
                x.FirstName.Contains(criteria.Keyword) ||
                x.LastName.Contains(criteria.Keyword) ||
                x.Line1.Contains(criteria.Keyword) ||
                x.Line2.Contains(criteria.Keyword) ||
                x.City.Contains(criteria.Keyword) ||
                x.CountryName.Contains(criteria.Keyword) ||
                x.PostalCode.Contains(criteria.Keyword));
        }

        if (!criteria.MemberId.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.MemberId == x.MemberId);
        }

        if (!criteria.CountryCodes.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.CountryCodes.Contains(x.CountryCode));
        }

        if (!criteria.RegionIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.RegionIds.Contains(x.RegionId));
        }

        if (!criteria.Cities.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.Cities.Contains(x.City));
        }

        if (!criteria.ObjectIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.ObjectIds.Contains(x.Id));
        }

        return query;
    }

    protected override IChangeToken CreateCacheToken(AddressSearchCriteria criteria)
    {
        var memberKey = criteria.MemberId ?? string.Empty;
        return GenericSearchCachingRegion<Address>.CreateChangeTokenForKey(memberKey);
    }

    protected override IList<SortInfo> BuildSortExpression(AddressSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(AddressEntity.CreatedDate), SortDirection = SortDirection.Descending },
                new SortInfo { SortColumn = nameof(AddressEntity.Id) },
            ];
        }

        return sortInfos;
    }
}
