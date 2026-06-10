using System;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.CustomerModule.Data.Caching;

public class OrganizationMembershipCacheRegion : CancellableCacheRegion<OrganizationMembershipCacheRegion>
{
    public static IChangeToken CreateChangeTokenForUser(string userId)
    {
        ArgumentNullException.ThrowIfNull(userId);

        return new CompositeChangeToken(
        [
            CreateChangeToken(),
            CreateChangeTokenForKey(userId),
        ]);
    }

    public static void ExpireByUserId(string userId)
    {
        ExpireTokenForKey(userId);
    }

    public static IChangeToken CreateChangeTokenForId(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return new CompositeChangeToken(
        [
            CreateChangeToken(),
            CreateChangeTokenForKey(id),
        ]);
    }

    public static void ExpireById(string id)
    {
        ExpireTokenForKey(id);
    }
}
