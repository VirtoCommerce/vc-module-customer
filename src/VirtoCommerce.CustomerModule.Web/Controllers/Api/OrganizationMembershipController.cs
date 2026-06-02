using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using Permissions = VirtoCommerce.CustomerModule.Core.ModuleConstants.Security.Permissions;

namespace VirtoCommerce.CustomerModule.Web.Controllers.Api;

[Authorize]
[Route("api/customer/organization-memberships")]
public class OrganizationMembershipController(IOrganizationMembershipService membershipService) : Controller
{
    /// <summary>Returns all organization memberships for a user.</summary>
    [HttpGet("user/{userId}")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<IList<OrganizationMembership>>> GetByUserId([FromRoute] string userId)
    {
        var result = await membershipService.GetByUserIdAsync(userId);
        return Ok(result);
    }

    /// <summary>Returns the membership record for a specific user/organization pair.</summary>
    [HttpGet("user/{userId}/org/{organizationId}")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<OrganizationMembership>> GetByUserAndOrg(
        [FromRoute] string userId,
        [FromRoute] string organizationId)
    {
        var result = await membershipService.GetByUserAndOrgAsync(userId, organizationId);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>Returns a single membership record by ID.</summary>
    [HttpGet("{id}")]
    [Authorize(Permissions.Read)]
    public async Task<ActionResult<OrganizationMembership>> GetById([FromRoute] string id)
    {
        var result = await membershipService.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>Creates a new organization membership.</summary>
    [HttpPost]
    [Authorize(Permissions.Update)]
    public async Task<ActionResult<OrganizationMembership>> Create([FromBody] OrganizationMembership membership)
    {
        membership.Id = null;
        await membershipService.SaveChangesAsync([membership]);
        return Ok(membership);
    }

    /// <summary>Updates an existing organization membership.</summary>
    [HttpPut("{id}")]
    [Authorize(Permissions.Update)]
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
    [Authorize(Permissions.Update)]
    public async Task<ActionResult<OrganizationMembership>> Lock(
        [FromRoute] string id,
        [FromBody] LockMembershipRequest request = null)
    {
        await membershipService.LockAsync(id, request?.LockoutEnd);
        var result = await membershipService.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Unlocks a membership.</summary>
    [HttpPost("{id}/unlock")]
    [Authorize(Permissions.Update)]
    public async Task<ActionResult<OrganizationMembership>> Unlock([FromRoute] string id)
    {
        await membershipService.UnlockAsync(id);
        var result = await membershipService.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Deletes organization memberships by IDs.</summary>
    [HttpDelete]
    [Authorize(Permissions.Delete)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete([FromQuery] string[] ids)
    {
        await membershipService.DeleteAsync(ids);
        return NoContent();
    }
}
