using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.Platform.Core.Caching;

namespace VirtoCommerce.CustomerModule.Data.Caching
{
    public class CustomerCacheRegion : CancellableCacheRegion<CustomerCacheRegion>
    {
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> _memberRegionTokenLookup = new ConcurrentDictionary<string, CancellationTokenSource>();

        public static IChangeToken CreateChangeToken(string[] memberIds)
        {
            if (memberIds == null)
            {
                throw new ArgumentNullException(nameof(memberIds));
            }
            var changeTokens = new List<IChangeToken>() { CreateChangeToken() };
            foreach (var memberId in memberIds)
            {
                changeTokens.Add(new CancellationChangeToken(_memberRegionTokenLookup.GetOrAdd(memberId, new CancellationTokenSource()).Token));
            }
            return new CompositeChangeToken(changeTokens);
        }

        public static void ExpireMemberById(string memberId)
        {
            if (!string.IsNullOrEmpty(memberId) && _memberRegionTokenLookup.TryRemove(memberId, out var token))
            {
                token.Cancel();
            }
        }
    }
}
