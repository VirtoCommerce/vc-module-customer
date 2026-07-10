using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Core.Model;

public class OrganizationMembershipSearchCriteria : SearchCriteriaBase
{
    /// <summary>
    /// Restrict to memberships of a single user.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Restrict to memberships of any of these users.
    /// </summary>
    public IList<string> UserIds { get; set; }

    /// <summary>
    /// Restrict to memberships in a single organization.
    /// </summary>
    public string OrganizationId { get; set; }

    /// <summary>
    /// Restrict to memberships in any of these organizations.
    /// </summary>
    public IList<string> OrganizationIds { get; set; }

    /// <summary>
    /// Restrict to memberships that carry at least one of these roles.
    /// </summary>
    public IList<string> RoleIds { get; set; }

    /// <summary>
    /// When true, return only memberships that are currently locked
    /// (<see cref="OrganizationMembership.IsLocked"/> and the lockout has not expired).
    /// </summary>
    public bool OnlyLocked { get; set; }

    /// <summary>
    /// When true, return only memberships that are currently NOT locked — i.e. active
    /// (not <see cref="OrganizationMembership.IsLocked"/>, or the lockout has already expired).
    /// </summary>
    public bool OnlyUnlocked { get; set; }
}
