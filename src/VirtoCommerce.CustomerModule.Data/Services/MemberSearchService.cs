using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Services.Indexed;
using VirtoCommerce.CustomerModule.Data.Caching;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Security.Caching;

namespace VirtoCommerce.CustomerModule.Data.Services
{
    public class MemberSearchService : IMemberSearchService
    {
        private readonly Func<IMemberRepository> _repositoryFactory;
        private readonly IMemberService _memberService;
        private readonly IIndexedMemberSearchService _indexedSearchService;
        private readonly IPlatformMemoryCache _platformMemoryCache;

        public MemberSearchService(
            Func<IMemberRepository> repositoryFactory
            , IMemberService memberService
            , IIndexedMemberSearchService indexedSearchService
            , IPlatformMemoryCache platformMemoryCache
            )
        {
            _repositoryFactory = repositoryFactory;
            _memberService = memberService;
            _indexedSearchService = indexedSearchService;
            _platformMemoryCache = platformMemoryCache;
        }

        #region IMemberSearchService Members

        public virtual Task<MemberSearchResult> SearchMembersAsync(MembersSearchCriteria criteria)
        {
            var result = !string.IsNullOrEmpty(criteria?.Keyword)
                ? IndexedSearchMembersAsync(criteria)
                : RegularSearchMembersAsync(criteria);

            return result;
        }
        #endregion


        protected virtual Task<MemberSearchResult> IndexedSearchMembersAsync(MembersSearchCriteria criteria)
        {
            return _indexedSearchService.SearchMembersAsync(criteria);
        }

        /// <summary>
        /// Search members in database by given criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        protected virtual async Task<MemberSearchResult> RegularSearchMembersAsync(MembersSearchCriteria criteria)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(SearchMembersAsync), criteria.GetCacheKey());
            return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                var result = AbstractTypeFactory<MemberSearchResult>.TryCreateInstance();
                cacheEntry.AddExpirationToken(CustomerSearchCacheRegion.CreateChangeToken());
                using (var repository = _repositoryFactory())
                {
                    //Optimize performance and CPU usage
                    repository.DisableChangesTracking();

                    var sortInfos = BuildSortExpression(criteria);
                    var query = BuildQuery(repository, criteria);

                    var needExecuteCount = criteria.Take == 0;

                    if (criteria.Take > 0)
                    {
                        var ids = await query.OrderBySortInfos(sortInfos).ThenBy(x => x.Id)
                                         .Select(x => x.Id)
                                         .Skip(criteria.Skip).Take(criteria.Take)
                                         .ToListAsync();

                        result.TotalCount = ids.Count;
                        // This reduces a load of a relational database by skipping count query in case of:
                        // - First page is reading (Skip is 0)
                        // - Count in reading result less than Take value.
                        if (criteria.Skip > 0 || result.TotalCount == criteria.Take)

                        {
                            needExecuteCount = true;
                        }
                        result.Results = (await _memberService.GetByIdsAsync(ids.ToArray(), criteria.ResponseGroup)).OrderBy(x => ids.IndexOf(x.Id)).ToList();

                        result.Results
                              .OfType<IHasSecurityAccounts>()
                              .SelectMany(x => x.SecurityAccounts)
                              .ToList()
                              .ForEach(x => cacheEntry.AddExpirationToken(SecurityCacheRegion.CreateChangeTokenForUser(x)));
                    }

                    if (needExecuteCount)
                    {
                        result.TotalCount = await query.CountAsync();
                    }
                    return result;
                }
            });
        }

        protected virtual IQueryable<MemberEntity> BuildQuery(IMemberRepository repository, MembersSearchCriteria criteria)
        {
            var query = repository.Members;

            if (!criteria.ObjectIds.IsNullOrEmpty())
            {
                query = query.Where(m => criteria.ObjectIds.Contains(m.Id));
            }

            if (!criteria.MemberTypes.IsNullOrEmpty())
            {
                query = query.Where(m => criteria.MemberTypes.Contains(m.MemberType));
            }

            if (!criteria.Groups.IsNullOrEmpty())
            {
                query = query.Where(m => m.Groups.Any(g => criteria.Groups.Contains(g.Group)));
            }

            if (criteria.MemberId != null)
            {
                //TODO: DeepSearch in specified member
                query = query.Where(m => m.MemberRelations
                    .Where(x => x.RelationType == RelationType.Membership.ToString())
                    .Any(r => r.AncestorId == criteria.MemberId));
            }
            else if (!criteria.DeepSearch)
            {
                query = query.Where(m => m.MemberRelations.All(x => x.RelationType != RelationType.Membership.ToString()));
            }

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(m => m.Name.Contains(criteria.Keyword) || m.Emails.Any(e => e.Address.Contains(criteria.Keyword)));
            }

            if (!criteria.OuterIds.IsNullOrEmpty())
            {
                query = query.Where(m => criteria.OuterIds.Contains(m.OuterId));
            }
            return query;
        }

        protected virtual IList<SortInfo> BuildSortExpression(MembersSearchCriteria criteria)
        {
            var sortInfos = criteria.SortInfos;
            if (sortInfos.IsNullOrEmpty())
            {
                sortInfos = new[] {
                            new SortInfo { SortColumn = nameof(Member.MemberType), SortDirection = SortDirection.Descending },
                            new SortInfo { SortColumn = nameof(Member.Name), SortDirection = SortDirection.Ascending },
                        };
            }
            return sortInfos;
        }

    }
}
