using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Security.Model;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace VirtoCommerce.CustomerModule.Data.Services.Security
{
    public static class CustomerSecurityErrorDescriber
    {
        public static TokenLoginResponse EmailVerificationIsRequired() => new()
        {
            Error = Errors.InvalidGrant,
            Code = nameof(EmailVerificationIsRequired).PascalToKebabCase(),
            ErrorDescription = "Email verification required. Please verify your email address."
        };
    }
}
