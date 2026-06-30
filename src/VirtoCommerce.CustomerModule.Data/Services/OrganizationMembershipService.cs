using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
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
    private readonly Func<IOrganizationMembershipSearchService> _searchServiceFactory;
    private readonly IMemberService _memberService;

    public OrganizationMembershipService(
        Func<ICustomerRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher,
        Func<IOrganizationMembershipSearchService> searchServiceFactory,
        IMemberService memberService)
        : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
        _repositoryFactory = repositoryFactory;
        _searchServiceFactory = searchServiceFactory;
        _memberService = memberService;
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

    public override Task DeleteAsync(IList<string> ids, bool softDelete = false)
    {
        return ids.IsNullOrEmpty() ? Task.CompletedTask : base.DeleteAsync(ids, softDelete);
    }

    public Task<OrganizationMembership> LockAsync(string id, DateTime? lockoutEnd = null)
        => SetLockStateAsync(id, isLocked: true, lockoutEnd: lockoutEnd);

    public Task<OrganizationMembership> UnlockAsync(string id)
        => SetLockStateAsync(id, isLocked: false, lockoutEnd: null);

    public async Task<IReadOnlyCollection<OrganizationRole>> GetRolesByUserAndOrgAsync(string userId, string organizationId)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(organizationId))
        {
            return [];
        }

        var orgTask = _memberService.GetByIdAsync(organizationId, memberType: nameof(Organization));
        var membershipTask = _searchServiceFactory().SearchAsync(new OrganizationMembershipSearchCriteria
        {
            UserId = userId,
            OrganizationId = organizationId,
            Take = 1,
        });

        await Task.WhenAll(orgTask, membershipTask);

        var organization = await orgTask as Organization;
        var membership = (await membershipTask).Results.FirstOrDefault();

        IEnumerable<OrganizationRole> orgRoles = organization?.Roles ?? [];
        var membershipRoles = membership?.Roles?
            .Select(r => new OrganizationRole { RoleId = r.RoleId, RoleName = r.RoleName }) ?? [];

        return orgRoles
            .Concat(membershipRoles)
            .DistinctBy(r => r.RoleId)
            .ToList();
    }

    public async Task<IReadOnlyCollection<string>> GetUserIdsByRoleInOrgAsync(string organizationId, IList<string> roleIds)
    {
        if (string.IsNullOrEmpty(organizationId) || roleIds.IsNullOrEmpty())
        {
            return [];
        }

        using var repository = _repositoryFactory();

        return await repository.OrganizationMemberships
            .Where(m => m.OrganizationId == organizationId)
            .Where(m => m.Roles.Any(r => roleIds.Contains(r.RoleId)))
            .Select(m => m.UserId)
            .Distinct()
            .ToListAsync();
    }

    [Obsolete("Use IOrganizationMembershipSearchService.SearchAsync instead.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public Task<OrganizationMembershipSearchResult> SearchAsync(OrganizationMembershipSearchCriteria criteria, bool clone = true)
    {
        return _searchServiceFactory().SearchAsync(criteria, clone);
    }

    [Obsolete("Use IOrganizationMembershipSearchService.SearchAsync with UserId and OrganizationId filters instead.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public async Task<OrganizationMembership> GetByUserAndOrgAsync(string userId, string organizationId)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(organizationId))
        {
            return null;
        }

        var result = await _searchServiceFactory().SearchAsync(new OrganizationMembershipSearchCriteria
        {
            UserId = userId,
            OrganizationId = organizationId,
            Take = 1,
        });

        return result.Results.FirstOrDefault();
    }

    [Obsolete("Use IOrganizationMembershipSearchService.SearchAsync with the OnlyLocked filter instead.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public async Task<IReadOnlyCollection<string>> GetLockedOrganizationIdsAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return [];
        }

        var result = await _searchServiceFactory().SearchAsync(new OrganizationMembershipSearchCriteria
        {
            UserId = userId,
            OnlyLocked = true,
            Take = int.MaxValue,
        });

        return result.Results
            .Select(x => x.OrganizationId)
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();
    }

    [Obsolete("Use IOrganizationMembershipSearchService.SearchAsync with Take = 0 and read TotalCount instead.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public async Task<int> CountByUserIdAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return 0;
        }

        var result = await _searchServiceFactory().SearchAsync(new OrganizationMembershipSearchCriteria
        {
            UserId = userId,
            Take = 0,
        });

        return result.TotalCount;
    }

    [Obsolete("Use IOrganizationMembershipSearchService.GetCountsByUserAsync instead.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public Task<IDictionary<string, int>> GetOrganizationCountsByUserAsync(string[] roleIds, string[] organizationIds = null, string[] userIds = null)
    {
        return _searchServiceFactory().GetCountsByUserAsync(new OrganizationMembershipSearchCriteria
        {
            RoleIds = roleIds,
            OrganizationIds = organizationIds,
            UserIds = userIds,
        });
    }

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
