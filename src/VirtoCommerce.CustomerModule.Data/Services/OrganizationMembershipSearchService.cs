using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.CustomerModule.Data.Services;

public class OrganizationMembershipSearchService(
    Func<ICustomerRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    IOrganizationMembershipService crudService,
    IOptions<CrudOptions> crudOptions,
    IMemberService memberService)
    : SearchService<OrganizationMembershipSearchCriteria, OrganizationMembershipSearchResult, OrganizationMembership, OrganizationMembershipEntity>
        (repositoryFactory, platformMemoryCache, crudService, crudOptions),
        IOrganizationMembershipSearchService
{
    private readonly IPlatformMemoryCache _platformMemoryCache = platformMemoryCache;

    public virtual async Task<IReadOnlyCollection<OrganizationRole>> GetRolesByUserAndOrgAsync(string userId, string organizationId)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(organizationId))
        {
            return [];
        }

        var orgTask = memberService.GetByIdAsync(organizationId, memberType: nameof(Organization));
        var membershipTask = SearchAsync(
            new OrganizationMembershipSearchCriteria
            {
                UserId = userId,
                OrganizationId = organizationId,
                Take = 1,
            });

        await Task.WhenAll(orgTask, membershipTask);

        var organization = orgTask.Result as Organization;
        var membership = membershipTask.Result.Results.FirstOrDefault();

        return MergeRoles(organization, membership);
    }

    public virtual async Task<IReadOnlyCollection<OrganizationRole>> GetRolesByUserAndOrgAsync(string organizationId, OrganizationMembership membership)
    {
        if (string.IsNullOrEmpty(organizationId))
        {
            return [];
        }

        var organization = await memberService.GetByIdAsync(organizationId, memberType: nameof(Organization)) as Organization;

        return MergeRoles(organization, membership);
    }

    private static List<OrganizationRole> MergeRoles(Organization organization, OrganizationMembership membership)
    {
        IEnumerable<OrganizationRole> orgRoles = organization?.Roles ?? [];
        var membershipRoles = membership?.Roles?.Select(ToOrganizationRole) ?? [];

        return orgRoles
            .Concat(membershipRoles)
            .DistinctBy(r => r.RoleId)
            .ToList();
    }

    public virtual async Task<IDictionary<string, IReadOnlyCollection<OrganizationRole>>> GetRolesForUsersInOrgAsync(
        IList<string> userIds, string organizationId)
    {
        if (userIds.IsNullOrEmpty() || string.IsNullOrEmpty(organizationId))
        {
            return new Dictionary<string, IReadOnlyCollection<OrganizationRole>>();
        }

        // Filter out null/empty and deduplicate defensively: Dictionary rejects a null key,
        // and a caller-supplied list with repeated ids would otherwise throw on ToDictionary below
        var distinctUserIds = userIds
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (distinctUserIds.Count == 0)
        {
            return new Dictionary<string, IReadOnlyCollection<OrganizationRole>>();
        }

        var orgTask = memberService.GetByIdAsync(organizationId, memberType: nameof(Organization));
        var membershipsTask = SearchAsync(
            new OrganizationMembershipSearchCriteria
            {
                UserIds = distinctUserIds,
                OrganizationId = organizationId,
                Take = distinctUserIds.Count,
            });

        await Task.WhenAll(orgTask, membershipsTask);

        var organization = orgTask.Result as Organization;
        IEnumerable<OrganizationRole> orgRoles = organization?.Roles ?? [];

        var membershipByUserId = membershipsTask.Result.Results.ToDictionary(m => m.UserId);

        return distinctUserIds.ToDictionary(
            userId => userId,
            userId =>
            {
                var membershipRoles = membershipByUserId.TryGetValue(userId, out var membership)
                    ? membership.Roles?.Select(ToOrganizationRole) ?? []
                    : [];

                return (IReadOnlyCollection<OrganizationRole>)orgRoles
                    .Concat(membershipRoles)
                    .DistinctBy(r => r.RoleId)
                    .ToList();
            });
    }

    public virtual async Task<IReadOnlyCollection<string>> GetUserIdsByRoleInOrgAsync(string organizationId, IList<string> roleIds)
    {
        if (string.IsNullOrEmpty(organizationId) || roleIds.IsNullOrEmpty())
        {
            return [];
        }

        using var repository = repositoryFactory();

        return await repository.OrganizationMemberships
            .Where(m => m.OrganizationId == organizationId)
            .Where(m => m.Roles.Any(r => roleIds.Contains(r.RoleId)))
            .Select(m => m.UserId)
            .Distinct()
            .ToListAsync();
    }

    public virtual Task<IReadOnlyCollection<string>> GetLockedOrganizationIdsAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return Task.FromResult<IReadOnlyCollection<string>>([]);
        }

        var cacheKey = CacheKey.With(GetType(), nameof(GetLockedOrganizationIdsAsync), userId);

        return _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
        {
            cacheEntry.AddExpirationToken(GenericSearchCachingRegion<OrganizationMembership>.CreateChangeToken());

            // OnlyLocked embeds DateTime.UtcNow, so a temporary lock (LockoutEnd) can naturally expire
            // without any write happening to bust the region token above — bound the staleness window
            cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

            using var repository = repositoryFactory();

            var criteria = new OrganizationMembershipSearchCriteria
            {
                UserId = userId,
                OnlyLocked = true,
            };

            return (IReadOnlyCollection<string>)await BuildQuery(repository, criteria)
                .Select(x => x.OrganizationId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToListAsync();
        });
    }

    public virtual async Task<IDictionary<string, int>> GetCountsByUserAsync(OrganizationMembershipSearchCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        using var repository = repositoryFactory();

        var grouped = await BuildQuery(repository, criteria)
            .GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToListAsync();

        return grouped.ToDictionary(x => x.UserId, x => x.Count);
    }

    private static OrganizationRole ToOrganizationRole(OrganizationMembershipRole source)
    {
        var role = AbstractTypeFactory<OrganizationRole>.TryCreateInstance();
        role.RoleId = source.RoleId;
        role.RoleName = source.RoleName;

        return role;
    }

    protected override IQueryable<OrganizationMembershipEntity> BuildQuery(IRepository repository, OrganizationMembershipSearchCriteria criteria)
    {
        var query = ((ICustomerRepository)repository).OrganizationMemberships;

        if (!criteria.ObjectIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.ObjectIds.Contains(x.Id));
        }

        if (!string.IsNullOrEmpty(criteria.UserId))
        {
            query = query.Where(x => x.UserId == criteria.UserId);
        }

        if (!criteria.UserIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.UserIds.Contains(x.UserId));
        }

        if (!string.IsNullOrEmpty(criteria.OrganizationId))
        {
            query = query.Where(x => x.OrganizationId == criteria.OrganizationId);
        }

        if (!criteria.OrganizationIds.IsNullOrEmpty())
        {
            query = query.Where(x => criteria.OrganizationIds.Contains(x.OrganizationId));
        }

        if (!criteria.RoleIds.IsNullOrEmpty())
        {
            query = query.Where(x => x.Roles.Any(r => criteria.RoleIds.Contains(r.RoleId)));
        }

        if (criteria.OnlyLocked || criteria.OnlyUnlocked)
        {
            var now = DateTime.UtcNow;

            if (criteria.OnlyLocked)
            {
                query = query.Where(x => x.IsLocked && (x.LockoutEnd == null || x.LockoutEnd > now));
            }

            if (criteria.OnlyUnlocked)
            {
                query = query.Where(x => !x.IsLocked || (x.LockoutEnd != null && x.LockoutEnd <= now));
            }
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(OrganizationMembershipSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(OrganizationMembershipEntity.CreatedDate) },
            ];
        }

        return sortInfos;
    }
}
