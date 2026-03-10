using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
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

        if (!criteria.MembersIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.MembersIds.Contains(x.MemberId));
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

        return query;
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
