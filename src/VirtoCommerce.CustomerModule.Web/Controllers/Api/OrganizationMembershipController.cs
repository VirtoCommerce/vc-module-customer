using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using OrgMembershipPermissions = VirtoCommerce.CustomerModule.Core.ModuleConstants.Security.OrganizationMembershipPermissions;

namespace VirtoCommerce.CustomerModule.Web.Controllers.Api;

[Authorize]
[Route("api/customer/organization-memberships")]
public class OrganizationMembershipController(IOrganizationMembershipService membershipService) : Controller
{
    /// <summary>Returns the count of organization memberships for a user (lightweight — for widget counters).</summary>
    [HttpGet("user/{userId}/count")]
    [Authorize(OrgMembershipPermissions.Read)]
    public async Task<ActionResult> CountByUserId([FromRoute] string userId)
    {
        var count = await membershipService.CountByUserIdAsync(userId);
        return Ok(new { count });
    }

    /// <summary>Returns paginated organization memberships.</summary>
    [HttpPost("search")]
    [Authorize(OrgMembershipPermissions.Read)]
    public async Task<ActionResult<OrganizationMembershipSearchResult>> Search([FromBody] OrganizationMembershipSearchCriteria criteria)
    {
        var result = await membershipService.SearchAsync(criteria);
        return Ok(result);
    }

    /// <summary>Returns the membership record for a specific user/organization pair.</summary>
    [HttpGet("user/{userId}/org/{organizationId}")]
    [Authorize(OrgMembershipPermissions.Read)]
    public async Task<ActionResult<OrganizationMembership>> GetByUserAndOrg(
        [FromRoute] string userId,
        [FromRoute] string organizationId)
    {
        var result = await membershipService.GetByUserAndOrgAsync(userId, organizationId);
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>Returns a single membership record by ID.</summary>
    [HttpGet("{id}")]
    [Authorize(OrgMembershipPermissions.Read)]
    public async Task<ActionResult<OrganizationMembership>> GetById([FromRoute] string id)
    {
        var result = (await membershipService.GetAsync([id])).FirstOrDefault();
        return result != null ? Ok(result) : NotFound();
    }

    /// <summary>Creates a new organization membership.</summary>
    [HttpPost]
    [Authorize(OrgMembershipPermissions.Create)]
    public async Task<ActionResult<OrganizationMembership>> Create([FromBody] OrganizationMembership membership)
    {
        membership.Id = null;
        await membershipService.SaveChangesAsync([membership]);
        return Ok(membership);
    }

    /// <summary>Updates an existing organization membership.</summary>
    [HttpPut("{id}")]
    [Authorize(OrgMembershipPermissions.Update)]
    public async Task<ActionResult<OrganizationMembership>> Update(
        [FromRoute] string id,
        [FromBody] OrganizationMembership membership)
    {
        membership.Id = id;
        await membershipService.SaveChangesAsync([membership]);
        return Ok(membership);
    }

    /// <summary>Locks a membership — user cannot sign in to this organization.</summary>
    [HttpPost("{id}/lock")]
    [Authorize(OrgMembershipPermissions.Update)]
    public async Task<ActionResult<OrganizationMembership>> Lock(
        [FromRoute] string id,
        [FromBody] LockMembershipRequest request = null)
    {
        return Ok(await membershipService.LockAsync(id, request?.LockoutEnd));
    }

    /// <summary>Unlocks a membership.</summary>
    [HttpPost("{id}/unlock")]
    [Authorize(OrgMembershipPermissions.Update)]
    public async Task<ActionResult<OrganizationMembership>> Unlock([FromRoute] string id)
    {
        return Ok(await membershipService.UnlockAsync(id));
    }

    /// <summary>Deletes organization memberships by IDs.</summary>
    [HttpDelete]
    [Authorize(OrgMembershipPermissions.Delete)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete([FromQuery] string[] ids)
    {
        await membershipService.DeleteAsync(ids);
        return NoContent();
    }
}
