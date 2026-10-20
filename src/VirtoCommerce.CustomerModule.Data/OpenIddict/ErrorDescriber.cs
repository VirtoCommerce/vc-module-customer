using System;
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

    public static TokenResponse UserIsLockedInOrganization(string organizationId) => new()
    {
        Error = Errors.InvalidGrant,
        Code = nameof(UserIsLockedInOrganization).ToSnakeCase(),
        ErrorDescription = $"Your access to organization '{organizationId}' has been blocked. Please contact your organization administrator.",
    };

    public static TokenResponse UserInvitationPendingInOrganization(string organizationId) => new()
    {
        Error = Errors.InvalidGrant,
        Code = nameof(UserInvitationPendingInOrganization).ToSnakeCase(),
        ErrorDescription = $"Your invitation to organization '{organizationId}' has not been accepted yet.",
    };

    public static TokenResponse UserIsRejectedInOrganization(string organizationId) => new()
    {
        Error = Errors.InvalidGrant,
        Code = nameof(UserIsRejectedInOrganization).ToSnakeCase(),
        ErrorDescription = $"Your access to organization '{organizationId}' has been rejected. Please contact your organization administrator.",
    };

    public static TokenResponse UserIsRemovedFromOrganization(string organizationId) => new()
    {
        Error = Errors.InvalidGrant,
        Code = nameof(UserIsRemovedFromOrganization).ToSnakeCase(),
        ErrorDescription = $"Your access to organization '{organizationId}' has been removed.",
    };

    [Obsolete("Use VirtoCommerce.Platform.Security.OpenIddict.SecurityErrorDescriber.GetLockoutError(user) or SecurityErrorDescriber.UserIsLockedOut() instead.", DiagnosticId = "VC0016", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public static TokenResponse UserIsLockedOut() => new()
    {
        Error = Errors.InvalidGrant,
        Code = nameof(UserIsLockedOut).ToSnakeCase(),
        ErrorDescription = "Your account has been locked out. Please contact your administrator.",
    };
}
