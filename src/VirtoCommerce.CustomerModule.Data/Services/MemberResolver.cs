using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerModule.Data.Services
{
    public class MemberResolver : IMemberResolver
    {
        private readonly IMemberService _memberService;
        private readonly Func<UserManager<ApplicationUser>> _userManagerFactory;
        private readonly IPlatformMemoryCache _platformMemoryCache;

        public MemberResolver(IMemberService memberService, Func<UserManager<ApplicationUser>> userManagerFactory, IPlatformMemoryCache platformMemoryCache)
        {
            _memberService = memberService;
            _userManagerFactory = userManagerFactory;
            _platformMemoryCache = platformMemoryCache;
        }

        public Task<Member> ResolveMemberByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            // Try to find contact
            var cacheKey = CacheKey.With(GetType(), nameof(ResolveMemberByIdAsync), userId);
            return _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, cacheOptions =>
            {
                cacheOptions.AddExpirationToken(CreateCacheToken(userId));
                return ResolveMemberByIdNoCacheAsync(userId);
            });
        }

        private static IChangeToken CreateCacheToken(string userId)
        {
            return CustomerCacheRegion.CreateChangeTokenForKey(userId);
        }

        private async Task<Member> ResolveMemberByIdNoCacheAsync(string userId)
        {
            var member = await _memberService.GetByIdAsync(userId);

            if (member == null)
            {
                using var userManager = _userManagerFactory();
                var user = await userManager.FindByIdAsync(userId);

                if (!string.IsNullOrEmpty(user?.MemberId))
                {
                    member = await _memberService.GetByIdAsync(user.MemberId);
                }
            }
            return member;
        }
    }
}
