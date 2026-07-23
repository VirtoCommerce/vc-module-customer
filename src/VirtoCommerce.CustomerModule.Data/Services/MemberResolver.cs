using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerModule.Data.Services
{
    public class MemberResolver(IMemberService memberService, Func<UserManager<ApplicationUser>> userManagerFactory, IHttpContextAccessor httpContextAccessor) : IMemberResolver
    {
        private const string RequestCacheKey = "MemberResolver:RequestCache";

        [Obsolete("Use new constructor without IPlatformMemoryCache argument", DiagnosticId = "VC0012", UrlFormat = "https://docs.virtocommerce.org/platform/user-guide/versions/virto3-products-versions/")]
        public MemberResolver(IMemberService memberService, Func<UserManager<ApplicationUser>> userManagerFactory, IPlatformMemoryCache platformMemoryCache)
            : this(memberService, userManagerFactory, (IHttpContextAccessor)null)
        {
        }

        public virtual Task<Member> ResolveMemberByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            // Per-request cache: getFullCart resolves the same shopper's userId many times per request,
            // and each uncached call constructs a fresh CustomUserManager, which takes a process-global
            // Meter lock unconditionally (.NET 9 UserManagerMetrics) -> lock convoy under load.
            var httpContext = httpContextAccessor?.HttpContext;
            if (httpContext is null)
            {
                // No ambient request (e.g. background job) - nothing to scope the cache to.
                return ResolveMemberByIdInternalAsync(userId);
            }

            var cache = GetOrCreateRequestCache(httpContext);

            // Lazy guarantees the resolution runs once even if two callers miss the same key concurrently.
            var lazyTask = cache.GetOrAdd(userId, static (id, resolver) => new Lazy<Task<Member>>(() => resolver.ResolveMemberByIdInternalAsync(id)), this);

            return lazyTask.Value;
        }

        private static ConcurrentDictionary<string, Lazy<Task<Member>>> GetOrCreateRequestCache(HttpContext httpContext)
        {
            // HttpContext.Items is not thread-safe; the O(1) container get-or-create runs under the lock.
            // The expensive resolution stays outside it, via GetOrAdd(...).Value on the ConcurrentDictionary.
            lock (httpContext.Items)
            {
                if (httpContext.Items.TryGetValue(RequestCacheKey, out var value)
                    && value is ConcurrentDictionary<string, Lazy<Task<Member>>> cache)
                {
                    return cache;
                }

                cache = new ConcurrentDictionary<string, Lazy<Task<Member>>>();
                httpContext.Items[RequestCacheKey] = cache;

                return cache;
            }
        }

        private async Task<Member> ResolveMemberByIdInternalAsync(string userId)
        {
            using var userManager = userManagerFactory();
            var user = await userManager.FindByIdAsync(userId);

            var memberId = user != null
                ? user.MemberId
                : userId;

            if (string.IsNullOrEmpty(memberId))
            {
                return null;
            }

            return await memberService.GetByIdAsync(memberId);
        }
    }
}
