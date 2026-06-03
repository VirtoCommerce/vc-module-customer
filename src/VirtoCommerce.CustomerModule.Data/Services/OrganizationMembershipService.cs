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
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CustomerModule.Data.Services;

public class OrganizationMembershipService(
    Func<ICustomerRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    IEventPublisher eventPublisher)
    : IOrganizationMembershipService
{
    public async Task<OrganizationMembership> GetByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        var cacheKey = CacheKey.With(GetType(), nameof(GetByIdAsync), id);

        return await platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
        {
            cacheEntry.AddExpirationToken(OrganizationMembershipCacheRegion.CreateChangeTokenForId(id));

            using var repository = repositoryFactory();
            var entity = await repository.OrganizationMemberships
                .Include(x => x.Roles)
                .FirstOrDefaultAsync(x => x.Id == id);

            var model = entity?.ToModel(new OrganizationMembership());
            if (model != null)
            {
                await ResolveOrganizationNamesAsync(repository, [model]);
            }

            return model;
        });
    }

    public async Task<OrganizationMembership> GetByUserAndOrgAsync(string userId, string organizationId)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(organizationId))
        {
            return null;
        }

        var cacheKey = CacheKey.With(GetType(), nameof(GetByUserAndOrgAsync), userId, organizationId);

        return await platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
        {
            cacheEntry.AddExpirationToken(OrganizationMembershipCacheRegion.CreateChangeTokenForUser(userId));

            using var repository = repositoryFactory();
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

        return await platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
        {
            cacheEntry.AddExpirationToken(OrganizationMembershipCacheRegion.CreateChangeTokenForUser(userId));

            var now = DateTime.UtcNow;
            using var repository = repositoryFactory();

            return (IReadOnlyCollection<string>)await repository.OrganizationMemberships
                .Where(x => x.UserId == userId
                         && x.IsLocked
                         && (x.LockoutEnd == null || x.LockoutEnd > now))
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

        return await platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
        {
            cacheEntry.AddExpirationToken(OrganizationMembershipCacheRegion.CreateChangeTokenForUser(userId));

            using var repository = repositoryFactory();

            return await repository.OrganizationMemberships.CountAsync(x => x.UserId == userId);
        });
    }

    public async Task<OrganizationMembershipSearchResult> SearchAsync(OrganizationMembershipSearchCriteria criteria)
    {
        if (string.IsNullOrEmpty(criteria?.UserId))
        {
            return new OrganizationMembershipSearchResult();
        }

        var cacheKey = CacheKey.With(GetType(), nameof(SearchAsync), criteria.UserId, criteria.Skip.ToString(), criteria.Take.ToString());

        return await platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
        {
            cacheEntry.AddExpirationToken(OrganizationMembershipCacheRegion.CreateChangeTokenForUser(criteria.UserId));

            using var repository = repositoryFactory();
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

    public async Task SaveChangesAsync(IList<OrganizationMembership> memberships)
    {
        if (memberships.IsNullOrEmpty())
        {
            return;
        }

        var pkMap = new PrimaryKeyResolvingMap();

        using var repository = repositoryFactory();

        var existingIds = memberships
            .Where(x => !string.IsNullOrEmpty(x.Id))
            .Select(x => x.Id)
            .ToArray();

        var existingEntities = await repository.OrganizationMemberships
            .Include(x => x.Roles)
            .Where(x => existingIds.Contains(x.Id))
            .ToListAsync();

        var changedEntries = new List<GenericChangedEntry<OrganizationMembership>>();

        foreach (var membership in memberships)
        {
            // Build an untracked source entity from the incoming model
            var sourceEntity = new OrganizationMembershipEntity().FromModel(membership, pkMap);

            var targetEntity = existingEntities.FirstOrDefault(x => x.Id == membership.Id);
            if (targetEntity == null)
            {
                var duplicate = await repository.OrganizationMemberships
                    .AnyAsync(x => x.UserId == membership.UserId && x.OrganizationId == membership.OrganizationId);
                if (duplicate)
                {
                    throw new InvalidOperationException(
                        $"Membership for user '{membership.UserId}' and organization '{membership.OrganizationId}' already exists.");
                }

                repository.Add(sourceEntity);
                changedEntries.Add(new GenericChangedEntry<OrganizationMembership>(membership, EntryState.Added));
            }
            else
            {
                var oldMembership = targetEntity.ToModel(new OrganizationMembership());
                changedEntries.Add(new GenericChangedEntry<OrganizationMembership>(membership, oldMembership, EntryState.Modified));

                // Patch the tracked entity to avoid EF duplicate-tracking conflict on child collections
                repository.TrackModifiedAsAddedForNewChildEntities(targetEntity);
                sourceEntity.Patch(targetEntity);
            }
        }

        await eventPublisher.Publish(new OrganizationMembershipChangingEvent(changedEntries));

        await repository.UnitOfWork.CommitAsync();
        pkMap.ResolvePrimaryKeys();

        await eventPublisher.Publish(new OrganizationMembershipChangedEvent(changedEntries));

        foreach (var m in memberships.Where(x => !string.IsNullOrEmpty(x.Id)))
        {
            OrganizationMembershipCacheRegion.ExpireById(m.Id);
        }

        InvalidateCacheForUsers(memberships.Select(x => x.UserId));
    }

    public Task LockAsync(string id, DateTime? lockoutEnd = null)
        => SetLockStateAsync(id, isLocked: true, lockoutEnd: lockoutEnd);

    public Task UnlockAsync(string id)
        => SetLockStateAsync(id, isLocked: false, lockoutEnd: null);

    private async Task SetLockStateAsync(string id, bool isLocked, DateTime? lockoutEnd)
    {
        using var repository = repositoryFactory();
        var entity = await repository.OrganizationMemberships.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            return;
        }

        var oldMembership = entity.ToModel(new OrganizationMembership());
        entity.IsLocked = isLocked;
        entity.LockoutEnd = lockoutEnd;
        var newMembership = entity.ToModel(new OrganizationMembership());

        var changedEntries = new List<GenericChangedEntry<OrganizationMembership>>
        {
            new(newMembership, oldMembership, EntryState.Modified)
        };

        await eventPublisher.Publish(new OrganizationMembershipChangingEvent(changedEntries));
        await repository.UnitOfWork.CommitAsync();

        // Invalidate cache before publishing the Changed event so that
        // event handlers reading the membership see the up-to-date state.
        OrganizationMembershipCacheRegion.ExpireById(id);
        OrganizationMembershipCacheRegion.ExpireByUserId(entity.UserId);

        await eventPublisher.Publish(new OrganizationMembershipChangedEvent(changedEntries));
    }

    public async Task DeleteAsync(IList<string> ids)
    {
        if (ids.IsNullOrEmpty())
        {
            return;
        }

        using var repository = repositoryFactory();

        var entities = await repository.OrganizationMemberships
            .Include(x => x.Roles)
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();

        var changedEntries = entities
            .Select(e => new GenericChangedEntry<OrganizationMembership>(
                e.ToModel(new OrganizationMembership()), EntryState.Deleted))
            .ToList();

        await eventPublisher.Publish(new OrganizationMembershipChangingEvent(changedEntries));

        foreach (var entity in entities)
        {
            repository.Remove(entity);
        }

        await repository.UnitOfWork.CommitAsync();

        foreach (var entity in entities)
        {
            OrganizationMembershipCacheRegion.ExpireById(entity.Id);
        }

        InvalidateCacheForUsers(entities.Select(x => x.UserId));

        await eventPublisher.Publish(new OrganizationMembershipChangedEvent(changedEntries));
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

    private static void InvalidateCacheForUsers(IEnumerable<string> userIds)
    {
        foreach (var userId in userIds.Distinct())
        {
            if (!string.IsNullOrEmpty(userId))
            {
                OrganizationMembershipCacheRegion.ExpireByUserId(userId);
            }
        }
    }
}
