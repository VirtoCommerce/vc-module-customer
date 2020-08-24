using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.CustomerModule.Data.Caching
{
    public class CustomerCacheRegion : CancellableCacheRegion<CustomerCacheRegion>
    {
        public static IChangeToken CreateChangeToken(string[] memberIds)
        {
            if (memberIds == null)
            {
                throw new ArgumentNullException(nameof(memberIds));
            }
            var changeTokens = new List<IChangeToken>() { CreateChangeToken() };
            foreach (var memberId in memberIds)
            {
                changeTokens.Add(CreateChangeTokenForKey(memberId));
            }
            return new CompositeChangeToken(changeTokens);
        }

        public static void ExpireMemberById(string memberId)
        {
            ExpireTokenForKey(memberId);
        }
    }
}
