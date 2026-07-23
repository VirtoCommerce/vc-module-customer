using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerModule.Data.Services
{
    public class MemberResolver(IMemberService memberService, Func<UserManager<ApplicationUser>> userManagerFactory, IHttpContextAccessor httpContextAccessor) : IMemberResolver
    {
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
            // Resolved per-call from the request scope (not ctor-injected): keeps this transient service
            // free of a captured Scoped dependency, so singleton consumers of IMemberResolver stay valid.
            var requestCache = httpContextAccessor?.HttpContext?.RequestServices?.GetService<IRequestScopedCache>();
            if (requestCache is null)
            {
                // No ambient request scope (e.g. background job) - nothing to scope the cache to.
                return ResolveMemberByIdInternalAsync(userId);
            }

            return requestCache.GetOrAddAsync($"{nameof(MemberResolver)}:{userId}", () => ResolveMemberByIdInternalAsync(userId));
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
