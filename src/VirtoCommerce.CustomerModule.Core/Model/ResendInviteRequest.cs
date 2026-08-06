namespace VirtoCommerce.CustomerModule.Core.Model;

public class ResendInviteRequest
{
    public string MembershipId { get; set; }

    public string UrlSuffix { get; set; }

    public string Message { get; set; }
}
