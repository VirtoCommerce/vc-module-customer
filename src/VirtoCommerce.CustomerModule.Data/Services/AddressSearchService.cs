using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
    IOptions<CrudOptions> crudOptions,
    IFavoriteAddressService favoriteAddressService)
    : SearchService<AddressSearchCriteria, AddressSearchResult, Address, AddressEntity>
        (repositoryFactory, platformMemoryCache, crudService, crudOptions),
        IAddressSearchService
{
    private const string IsFavoriteSortColumn = "IsFavorite";

    protected override async Task<GenericSearchResult<string>> SearchIdsNoCacheAsync(AddressSearchCriteria criteria)
    {
        var result = AbstractTypeFactory<GenericSearchResult<string>>.TryCreateInstance();
        result.Results = Array.Empty<string>();

        using var repository = repositoryFactory();
        var query = BuildQuery(repository, criteria);
        var needExecuteCount = criteria.Take == 0;

        if (criteria.Take > 0)
        {
            IOrderedQueryable<AddressEntity> orderedQuery = null;

            var sortInfos = BuildSortExpression(criteria);
            var isFavoriteSortInfo = sortInfos.FirstOrDefault(x => x.SortColumn.EqualsIgnoreCase(IsFavoriteSortColumn));
            if (isFavoriteSortInfo != null)
            {
                orderedQuery = await BuildQueryOrderedByFavoritesAsync(criteria, query, orderedQuery, sortInfos, isFavoriteSortInfo);
            }
            else
            {
                orderedQuery = query
                    .OrderBySortInfos(sortInfos)
                    .ThenBy(x => x.Id);
            }

            result.Results = await orderedQuery
                .Select(x => x.Id)
                .Skip(criteria.Skip)
                .Take(criteria.Take)
                .ToListAsync();

            result.TotalCount = result.Results.Count;

            // This reduces a load of a relational database by skipping count query in case of:
            // - First page is reading (Skip is 0)
            // - Count in reading result less than Take value.
            if (criteria.Skip > 0 || result.TotalCount == criteria.Take)
            {
                needExecuteCount = true;
            }
        }

        if (needExecuteCount)
        {
            result.TotalCount = await query.CountAsync();
        }

        return result;
    }

    private async Task<IOrderedQueryable<AddressEntity>> BuildQueryOrderedByFavoritesAsync(AddressSearchCriteria criteria, IQueryable<AddressEntity> query, IOrderedQueryable<AddressEntity> orderedQuery, IList<SortInfo> sortInfos, SortInfo isFavoriteSortInfo)
    {
        var favoriteAddressIds = await favoriteAddressService.GetFavoriteAddressIdsAsync(criteria.UserId);

        var basicSortInfos = GetBasicSortInfos(sortInfos, isFavoriteSortInfo);

        if (isFavoriteSortInfo.SortDirection == SortDirection.Descending)
        {
            orderedQuery = query
                .OrderByDescending(x => favoriteAddressIds.Contains(x.Id));
        }
        else
        {
            orderedQuery = query
                .OrderBy(x => favoriteAddressIds.Contains(x.Id));
        }

        orderedQuery = orderedQuery.ThenBySortInfos(basicSortInfos).ThenBy(x => x.Id);

        return orderedQuery;
    }

    private static SortInfo[] GetBasicSortInfos(IList<SortInfo> sortInfos, SortInfo isFavoriteSortInfo)
    {
        var basicSortInfos = sortInfos.ToList();
        basicSortInfos.Remove(isFavoriteSortInfo);
        return basicSortInfos.ToArray();
    }

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
            ];
        }

        return sortInfos;
    }
}
