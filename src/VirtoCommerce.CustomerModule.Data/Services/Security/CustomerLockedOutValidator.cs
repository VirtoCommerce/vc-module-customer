using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Security.Model;
using VirtoCommerce.Platform.Security.Services;
using VirtoCommerce.StoreModule.Core.Services;
using StoreSettings = VirtoCommerce.StoreModule.Core.ModuleConstants.Settings;

namespace VirtoCommerce.CustomerModule.Data.Services.Security
{
    public class CustomerLockedOutValidator : IUserSignInValidator
    {
        public int Priority { get; set; } = 1;

        private readonly IStoreService _storeService;
        private readonly IMemberService _memberService;

        public CustomerLockedOutValidator(IStoreService storeService, IMemberService memberService)
        {
            _storeService = storeService;
            _memberService = memberService;
        }

        public async Task<IList<TokenLoginResponse>> ValidateUserAsync(ApplicationUser user, SignInResult signInResult, IDictionary<string, object> context)
        {
            var result = new List<TokenLoginResponse>();

            var explicitErrors = false;
            if (context.TryGetValue("explicitErrors", out var explicitErrorsValue))
            {
                explicitErrors = (bool)explicitErrorsValue;
            }

            if (!explicitErrors)
            {
                return result;
            }

            if (context.TryGetValue("storeId", out var storeIdValue) && storeIdValue is string storeId)
            {
                var store = await _storeService.GetNoCloneAsync(storeId);
                if (store != null)
                {
                    var member = await _memberService.GetByIdAsync(user.MemberId);

                    if (member != null && signInResult.IsLockedOut)
                    {
                        var locked = member.Status == "Locked"
                            && !user.EmailConfirmed
                            && store.Settings.GetValue<bool>(StoreSettings.General.EmailVerificationEnabled)
                            && store.Settings.GetValue<bool>(StoreSettings.General.EmailVerificationRequired);

                        if (locked)
                        {
                            result.Add(CustomerSecurityErrorDescriber.EmailVerificationIsRequired());
                        }
                    }
                }
            }

            return result;
        }
    }
}
