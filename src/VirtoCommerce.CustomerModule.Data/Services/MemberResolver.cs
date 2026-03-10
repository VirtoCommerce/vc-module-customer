using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerModule.Data.Services
{
    public class MemberResolver(IMemberService memberService, Func<UserManager<ApplicationUser>> userManagerFactory) : IMemberResolver
    {
        [Obsolete("Use new constructor without IPlatformMemoryCache argument", DiagnosticId = "VC0012", UrlFormat = "https://docs.virtocommerce.org/platform/user-guide/versions/virto3-products-versions/")]
        public MemberResolver(IMemberService memberService, Func<UserManager<ApplicationUser>> userManagerFactory, IPlatformMemoryCache platformMemoryCache)
            : this(memberService, userManagerFactory)
        {
        }

        public virtual Task<Member> ResolveMemberByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return ResolveMemberByIdInternalAsync(userId);
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
