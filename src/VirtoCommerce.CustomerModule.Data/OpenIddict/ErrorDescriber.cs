using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Security.OpenIddict;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace VirtoCommerce.CustomerModule.Data.OpenIddict;

public static class ErrorDescriber
{
    public static TokenResponse InvalidOrganizationId(string organizationId) => new()
    {
        Error = Errors.InvalidGrant,
        Code = nameof(InvalidOrganizationId).ToSnakeCase(),
        ErrorDescription = $"Access denied. You cannot switch to organization '{organizationId}'.",
    };


    public static TokenResponse InvalidParamOrganizationId() => new()
    {
        Error = Errors.InvalidRequest,
        Code = nameof(InvalidParamOrganizationId).ToSnakeCase(),
        ErrorDescription = $"You cannot use 'impersonate' grant and 'organization_id' param.",
    };
}
