using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.CustomerModule.Core.Services;

public interface IOrganizationMembershipService
    : ICrudService<OrganizationMembership>,
      ISearchService<OrganizationMembershipSearchCriteria, OrganizationMembershipSearchResult, OrganizationMembership>
{
    Task<OrganizationMembership> LockAsync(string id, DateTime? lockoutEnd = null);

    Task<OrganizationMembership> UnlockAsync(string id);

    Task<IReadOnlyCollection<OrganizationRole>> GetRolesByUserAndOrgAsync(string userId, string organizationId);

    Task<IReadOnlyCollection<string>> GetUserIdsByRoleInOrgAsync(string organizationId, IList<string> roleIds);

    [Obsolete("Use IOrganizationMembershipSearchService.SearchAsync instead.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    new Task<OrganizationMembershipSearchResult> SearchAsync(OrganizationMembershipSearchCriteria criteria, bool clone = true);

    [Obsolete("Use IOrganizationMembershipSearchService.SearchAsync with UserId and OrganizationId filters instead.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    Task<OrganizationMembership> GetByUserAndOrgAsync(string userId, string organizationId);

    [Obsolete("Use IOrganizationMembershipSearchService.SearchAsync with the OnlyLocked filter instead.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    Task<IReadOnlyCollection<string>> GetLockedOrganizationIdsAsync(string userId);

    [Obsolete("Use IOrganizationMembershipSearchService.SearchAsync with Take = 0 and read TotalCount instead.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    Task<int> CountByUserIdAsync(string userId);

    [Obsolete("Use IOrganizationMembershipSearchService.GetCountsByUserAsync instead.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    Task<IDictionary<string, int>> GetOrganizationCountsByUserAsync(string[] roleIds, string[] organizationIds = null, string[] userIds = null);
}
