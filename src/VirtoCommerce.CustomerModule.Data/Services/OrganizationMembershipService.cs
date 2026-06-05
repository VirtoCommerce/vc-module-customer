using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.Caching;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.CustomerModule.Data.Services;

public class OrganizationMembershipService
    : CrudService<
        OrganizationMembership,
        OrganizationMembershipEntity,
        OrganizationMembershipChangingEvent,
        OrganizationMembershipChangedEvent>,
    IOrganizationMembershipService
{
    private readonly Func<ICustomerRepository> _repositoryFactory;
    private readonly IPlatformMemoryCache _platformMemoryCache;

    public OrganizationMembershipService(
        Func<ICustomerRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher)
        : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
        _repositoryFactory = repositoryFactory;
        _platformMemoryCache = platformMemoryCache;
    }

    protected override async Task<IList<OrganizationMembershipEntity>> LoadEntities(
        IRepository repository, IList<string> ids, string responseGroup)
    {
        return await ((ICustomerRepository)repository).OrganizationMemberships
            .Include(x => x.Roles)
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();
    }

    protected override async Task BeforeSaveChanges(IList<OrganizationMembership> models)
    {
        using var repository = _repositoryFactory();
        foreach (var model in models.Where(x => x.IsTransient()))
        {
            var duplicate = await repository.OrganizationMemberships
                .AnyAsync(x => x.UserId == model.UserId && x.OrganizationId == model.OrganizationId);

            if (duplicate)
            {
                throw new InvalidOperationException(
                    $"Membership for user '{model.UserId}' and organization '{model.OrganizationId}' already exists.");
            }
        }
    }

    protected override async Task<IList<OrganizationMembership>> GetByIdsNoCache(IList<string> ids, string responseGroup)
    {
        var models = await base.GetByIdsNoCache(ids, responseGroup);

        using var repository = _repositoryFactory();
        await ResolveOrganizationNamesAsync(repository, models);

        return models;
    }

    protected override void ClearCache(IList<OrganizationMembership> models)
    {
        base.ClearCache(models);

        foreach (var model in models)
        {
            if (!string.IsNullOrEmpty(model.Id))
            {
                OrganizationMembershipCacheRegion.ExpireById(model.Id);
            }

            if (!string.IsNullOrEmpty(model.UserId))
            {
                OrganizationMembershipCacheRegion.ExpireByUserId(model.UserId);
            }
        }
    }

    protected override void ClearSearchCache(IList<OrganizationMembership> models)
    {
        foreach (var model in models.Where(x => !string.IsNullOrEmpty(x.UserId)))
        {
            OrganizationMembershipCacheRegion.ExpireByUserId(model.UserId);
        }
    }

    public override Task DeleteAsync(IList<string> ids, bool softDelete = false)
    {
        return ids.IsNullOrEmpty() ? Task.CompletedTask : base.DeleteAsync(ids, softDelete);
    }

    public async Task<OrganizationMembershipSearchResult> SearchAsync(
        OrganizationMembershipSearchCriteria criteria, bool clone = true)
    {
        if (string.IsNullOrEmpty(criteria?.UserId))
        {
            return new OrganizationMembershipSearchResult();
        }

        var cacheKey = CacheKey.With(GetType(), nameof(SearchAsync),
            criteria.UserId, criteria.Skip.ToString(), criteria.Take.ToString());

        return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
        {
            cacheEntry.AddExpirationToken(
                OrganizationMembershipCacheRegion.CreateChangeTokenForUser(criteria.UserId));

            using var repository = _repositoryFactory();
            var query = repository.OrganizationMemberships.Where(x => x.UserId == criteria.UserId);

            var result = new OrganizationMembershipSearchResult
            {
                TotalCount = await query.CountAsync(),
                Results = await query
                    .Include(x => x.Roles)
                    .OrderBy(x => x.CreatedDate)
                    .Skip(criteria.Skip)
                    .Take(criteria.Take)
                    .Select(e => e.ToModel(new OrganizationMembership()))
                    .ToListAsync()
            };

            await ResolveOrganizationNamesAsync(repository, result.Results);

            return result;
        });
    }

    public async Task<OrganizationMembership> GetByUserAndOrgAsync(string userId, string organizationId)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(organizationId))
        {
            return null;
        }

        var cacheKey = CacheKey.With(GetType(), nameof(GetByUserAndOrgAsync), userId, organizationId);

        return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
        {
            cacheEntry.AddExpirationToken(
                OrganizationMembershipCacheRegion.CreateChangeTokenForUser(userId));

            using var repository = _repositoryFactory();
            var entity = await repository.OrganizationMemberships
                .Include(x => x.Roles)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.OrganizationId == organizationId);

            var model = entity?.ToModel(new OrganizationMembership());
            if (model != null)
            {
                await ResolveOrganizationNamesAsync(repository, [model]);
            }

            return model;
        });
    }

    public async Task<IReadOnlyCollection<string>> GetLockedOrganizationIdsAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return [];
        }

        var cacheKey = CacheKey.With(GetType(), nameof(GetLockedOrganizationIdsAsync), userId);

        return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
        {
            cacheEntry.AddExpirationToken(
                OrganizationMembershipCacheRegion.CreateChangeTokenForUser(userId));

            var now = DateTime.UtcNow;
            using var repository = _repositoryFactory();

            return (IReadOnlyCollection<string>)await repository.OrganizationMemberships
                .Where(x => x.UserId == userId &&
                            x.IsLocked &&
                            (x.LockoutEnd == null || x.LockoutEnd > now))
                .Select(x => x.OrganizationId)
                .ToListAsync();
        });
    }

    public async Task<int> CountByUserIdAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return 0;
        }

        var cacheKey = CacheKey.With(GetType(), nameof(CountByUserIdAsync), userId);

        return await _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
        {
            cacheEntry.AddExpirationToken(
                OrganizationMembershipCacheRegion.CreateChangeTokenForUser(userId));

            using var repository = _repositoryFactory();

            return await repository.OrganizationMemberships.CountAsync(x => x.UserId == userId);
        });
    }

    public Task<OrganizationMembership> LockAsync(string id, DateTime? lockoutEnd = null)
        => SetLockStateAsync(id, isLocked: true, lockoutEnd: lockoutEnd);

    public Task<OrganizationMembership> UnlockAsync(string id)
        => SetLockStateAsync(id, isLocked: false, lockoutEnd: null);

    private async Task<OrganizationMembership> SetLockStateAsync(string id, bool isLocked, DateTime? lockoutEnd)
    {
        var model = (await GetAsync([id])).FirstOrDefault();
        if (model == null)
        {
            return null;
        }

        model.IsLocked = isLocked;
        model.LockoutEnd = lockoutEnd;

        await SaveChangesAsync([model]);

        return model;
    }

    private static async Task ResolveOrganizationNamesAsync(
        ICustomerRepository repository,
        IList<OrganizationMembership> models)
    {
        var orgIds = models
            .Where(x => !string.IsNullOrEmpty(x.OrganizationId))
            .Select(x => x.OrganizationId)
            .Distinct()
            .ToArray();

        if (orgIds.Length == 0)
        {
            return;
        }

        var orgNames = await repository.Organizations
            .Where(o => orgIds.Contains(o.Id))
            .ToDictionaryAsync(o => o.Id, o => o.Name);

        foreach (var model in models)
        {
            if (string.IsNullOrEmpty(model.OrganizationName) &&
                !string.IsNullOrEmpty(model.OrganizationId) &&
                orgNames.TryGetValue(model.OrganizationId, out var name))
            {
                model.OrganizationName = name;
            }
        }
    }
}
