using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

public class OrganizationMembershipSearchService(
    Func<ICustomerRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    IOrganizationMembershipService crudService,
    IOptions<CrudOptions> crudOptions)
    : SearchService<OrganizationMembershipSearchCriteria, OrganizationMembershipSearchResult, OrganizationMembership, OrganizationMembershipEntity>
        (repositoryFactory, platformMemoryCache, crudService, crudOptions),
        IOrganizationMembershipSearchService
{
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

        if (criteria.OnlyLocked)
        {
            var now = DateTime.UtcNow;
            query = query.Where(x => x.IsLocked && (x.LockoutEnd == null || x.LockoutEnd > now));
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
