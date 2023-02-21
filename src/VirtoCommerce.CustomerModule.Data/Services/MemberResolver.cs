using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerModule.Data.Services
{
    public class MemberResolver : IMemberResolver
    {
        private readonly IMemberService _memberService;
        private readonly Func<UserManager<ApplicationUser>> _userManagerFactory;

        public MemberResolver(IMemberService memberService, Func<UserManager<ApplicationUser>> userManagerFactory)
        {
            _memberService = memberService;
            _userManagerFactory = userManagerFactory;
        }

        public async Task<Member> ResolveMemberByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            // Try to find contact
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
