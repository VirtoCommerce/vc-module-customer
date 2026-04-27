using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

    protected override IQueryable<AddressEntity> BuildQuery(IRepository repository, AddressSearchCriteria criteria)
    {
        var query = ((IMemberRepository)repository).Addresses;

        return ApplyFilters(query, criteria);
    }

    protected virtual IQueryable<AddressEntity> ApplyFilters(IQueryable<AddressEntity> query, AddressSearchCriteria criteria, string excludeFacet = null)
    {
        if (!criteria.Keyword.IsNullOrEmpty())
        {
            query = query.Where(x =>
                x.Name.Contains(criteria.Keyword) ||
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
            query = query.Where(x => x.MemberId == criteria.MemberId);
        }

        if (excludeFacet != "CountryCode" && !criteria.CountryCodes.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.CountryCodes.Contains(x.CountryCode));
        }

        if (excludeFacet != "RegionId" && !criteria.RegionIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.RegionIds.Contains(x.RegionId));
        }

        if (excludeFacet != "City" && !criteria.Cities.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.Cities.Contains(x.City));
        }

        if (!criteria.ObjectIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.ObjectIds.Contains(x.Id));
        }

        return query;
    }

    protected override async Task<AddressSearchResult> ProcessSearchResultAsync(AddressSearchResult result, AddressSearchCriteria criteria)
    {
        using var memberRepository = repositoryFactory();

        var baseQuery = memberRepository.Addresses.AsQueryable();

        result.Facets = new AddressFacetResult
        {
            Countries = await BuildFacetItemsAsync(
                    nameof(AddressEntity.CountryCode),
                    baseQuery,
                    criteria,
                    criteria.CountryCodes,
                    x => !string.IsNullOrEmpty(x.CountryCode),
                    x => x.CountryCode,
                    g => new AddressFacetItem
                    {
                        Value = g.Key,
                        Count = g.Count(),
                        CountryCode = g.Key,
                    }),

            Regions = await BuildFacetItemsAsync(
                    nameof(AddressEntity.RegionId),
                    baseQuery,
                    criteria,
                    criteria.RegionIds,
                    x => !string.IsNullOrEmpty(x.RegionId),
                    x => new { x.CountryCode, x.RegionId },
                    g => new AddressFacetItem
                    {
                        Value = g.Key.RegionId,
                        Count = g.Count(),
                        CountryCode = g.Key.CountryCode,
                        RegionId = g.Key.RegionId,
                    }),

            Cities = await BuildFacetItemsAsync(
                    nameof(AddressEntity.City),
                    baseQuery,
                    criteria,
                    criteria.Cities,
                    x => !string.IsNullOrEmpty(x.City),
                    x => x.City,
                    g => new AddressFacetItem
                    {
                        Value = g.Key,
                        Label = g.Key,
                        Count = g.Count(),
                    }),
        };

        return await base.ProcessSearchResultAsync(result, criteria);
    }

    protected virtual async Task<IList<AddressFacetItem>> BuildFacetItemsAsync<TKey>(
            string fieldName,
            IQueryable<AddressEntity> source,
            AddressSearchCriteria criteria,
            IList<string> appliedValues,
            Expression<Func<AddressEntity, bool>> keyFilter,
            Expression<Func<AddressEntity, TKey>> keySelector,
            Expression<Func<IGrouping<TKey, AddressEntity>, AddressFacetItem>> itemsSelector)
    {
        var query = ApplyFilters(source, criteria, excludeFacet: fieldName);

        var facetItems = await query
            .Where(keyFilter)
            .GroupBy(keySelector)
            .Select(itemsSelector)
            .OrderBy(x => x.Value)
            .ToListAsync();

        var applied = new HashSet<string>(appliedValues ?? [], StringComparer.OrdinalIgnoreCase);

        foreach (var item in facetItems)
        {
            item.IsApplied = applied.Contains(item.Value);
        }

        return facetItems;
    }

    protected override IChangeToken CreateCacheToken(AddressSearchCriteria criteria)
    {
        var memberKey = criteria.MemberId ?? string.Empty;
        var memberToken = GenericSearchCachingRegion<Address>.CreateChangeTokenForKey(memberKey);

        if (criteria.UserId.IsNullOrEmpty())
        {
            return memberToken;
        }

        var userToken = GenericSearchCachingRegion<Address>.CreateChangeTokenForKey(criteria.UserId);

        return new CompositeChangeToken([memberToken, userToken]);
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

    protected override async Task<IOrderedQueryable<AddressEntity>> GetOrderedQueryAsync(IQueryable<AddressEntity> query, AddressSearchCriteria criteria)
    {
        IOrderedQueryable<AddressEntity> orderedQuery;

        var sortInfos = BuildSortExpression(criteria);
        var isFavoriteSortInfo = sortInfos.FirstOrDefault(x => x.SortColumn.EqualsIgnoreCase(IsFavoriteSortColumn));
        if (isFavoriteSortInfo != null)
        {
            orderedQuery = await BuildQueryOrderedByFavoritesAsync(criteria, query, sortInfos, isFavoriteSortInfo);
        }
        else
        {
            orderedQuery = await base.GetOrderedQueryAsync(query, criteria);
        }

        return orderedQuery;
    }

    private async Task<IOrderedQueryable<AddressEntity>> BuildQueryOrderedByFavoritesAsync(AddressSearchCriteria criteria, IQueryable<AddressEntity> query, IList<SortInfo> sortInfos, SortInfo isFavoriteSortInfo)
    {
        var favoriteAddressIds = await favoriteAddressService.GetFavoriteAddressIdsAsync(criteria.UserId);

        IOrderedQueryable<AddressEntity> orderedQuery;

        if (isFavoriteSortInfo.SortDirection == SortDirection.Descending)
        {
            orderedQuery = query.OrderByDescending(x => favoriteAddressIds.Contains(x.Id));
        }
        else
        {
            orderedQuery = query.OrderBy(x => favoriteAddressIds.Contains(x.Id));
        }

        var basicSortInfos = GetBasicSortInfos(sortInfos, isFavoriteSortInfo);

        orderedQuery = orderedQuery
            .ThenBySortInfos(basicSortInfos)
            .ThenBy(x => x.Id);

        return orderedQuery;
    }

    private static SortInfo[] GetBasicSortInfos(IList<SortInfo> sortInfos, SortInfo isFavoriteSortInfo)
    {
        var basicSortInfos = sortInfos.ToList();
        basicSortInfos.Remove(isFavoriteSortInfo);
        return basicSortInfos.ToArray();
    }
}
