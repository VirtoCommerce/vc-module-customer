namespace VirtoCommerce.CustomerModule.Core.Model;

public class ChangeMembershipStatusRequest
{
    /// <summary>
    /// New organization-specific lifecycle status override. Pass null/empty to clear the override
    /// and fall back to the member's global status. Independent of locking — use the lock/unlock
    /// endpoints to suspend/restore access.
    /// </summary>
    public string Status { get; set; }
}
