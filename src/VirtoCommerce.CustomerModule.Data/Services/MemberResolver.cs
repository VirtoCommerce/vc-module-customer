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
        private readonly UserManager<ApplicationUser> _userManager;

        public MemberResolver(IMemberService memberService, Func<UserManager<ApplicationUser>> userManager)
        {
            _memberService = memberService;
            _userManager = userManager();
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
                var user = await _userManager.FindByIdAsync(userId);

                if (user != null && user.MemberId != null)
                {
                    member = await _memberService.GetByIdAsync(user.MemberId);
                }
            }
            return member;
        }
    }
}
