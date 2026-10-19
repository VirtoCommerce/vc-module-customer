using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Moq;
using OpenIddict.Abstractions;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.OpenIddict;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.OpenIddict;
using Xunit;
using static OpenIddict.Abstractions.OpenIddictConstants.Errors;
using static VirtoCommerce.CustomerModule.Core.ModuleConstants.MembershipStatuses;
using static VirtoCommerce.CustomerModule.Core.ModuleConstants.Security.Claims;
using static VirtoCommerce.Platform.Core.Security.SignInLog.SignInFailureReason;
using GrantTypes = OpenIddict.Abstractions.OpenIddictConstants.GrantTypes;
using PlatformGrantTypes = VirtoCommerce.Platform.Core.PlatformConstants.Security.GrantTypes;

namespace VirtoCommerce.CustomerModule.Tests;

public class OrganizationIdRequestValidatorTests
{
    private const string OrgId = "org1";
    private const string OrgId2 = "org2";
    private const string MemberId = "member1";
    private const string UserId = "user1";

    private readonly Mock<IMemberService> _memberServiceMock = new();
    private readonly Mock<IOrganizationMembershipSearchService> _membershipServiceMock = new();
    private readonly Contact _contact = new() { Id = MemberId, Organizations = [] };
    private readonly ApplicationUser _user = new() { Id = UserId, MemberId = MemberId };
    private readonly List<OrganizationMembership> _memberships = [];

    public OrganizationIdRequestValidatorTests()
    {
        _memberServiceMock
            .Setup(x => x.GetByIdAsync(MemberId, responseGroup: null, memberType: null))
            .ReturnsAsync(_contact);

        _membershipServiceMock
            .Setup(x => x.SearchAsync(It.IsAny<OrganizationMembershipSearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync((OrganizationMembershipSearchCriteria criteria, bool _) => new OrganizationMembershipSearchResult
            {
                Results = _memberships
                    .Where(x => x.UserId == criteria.UserId &&
                                (x.OrganizationId == criteria.OrganizationId || criteria.OrganizationIds?.Contains(x.OrganizationId) == true))
                    .ToList(),
            });
    }

    [Fact]
    public async Task ValidateAsync_NoOrganizationId_NoUser_ReturnsEmpty()
    {
        // Arrange
        var context = BuildContext(user: null, GrantTypes.RefreshToken);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ValidateAsync_NoExplicitOrgId_AutoDetectsFromMember_ActiveMembership_ReturnsEmpty()
    {
        // Arrange
        AddMembership(OrgId);

        var context = BuildContext(_user, GrantTypes.RefreshToken);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ValidateAsync_RefreshTokenGrant_NoExplicitOrgId_LockedInSoleOrg_ClearsOrgIdAndReturnsEmpty()
    {
        // Arrange — token rotation keeps the org from the refresh token, and the member is now locked in it with no other org to fall back to.
        AddMembership(OrgId, isLocked: true);

        var context = BuildContext(_user, GrantTypes.RefreshToken);
        context.Principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(OrganizationId, OrgId)], "test"));

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
        Assert.Null(context.Request.GetParameter(Parameters.OrganizationId));
        Assert.Null(context.Principal.FindFirstValue(OrganizationId));
    }

    [Fact]
    public async Task ValidateAsync_NoExplicitOrgId_MemberHasNoOrganizations_ReturnsEmpty()
    {
        // Arrange
        var context = BuildContext(_user, GrantTypes.RefreshToken);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(null, true, "user_is_locked_in_organization")]
    [InlineData(Rejected, false, "user_is_rejected_in_organization")]
    public async Task ValidateAsync_RefreshTokenGrantExplicitSwitchToUnavailableOrg_ReturnsOrgError(string status, bool isLocked, string expectedCode)
    {
        // Arrange — refresh_token with organization_id (switchOrganization()) is a deliberate switch, so it is blocked instead of falling back to a different org.
        AddMembership(OrgId, status, isLocked);
        AddMembership(OrgId2);

        var context = BuildContext(_user, GrantTypes.RefreshToken, OrgId);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Single(result);
        Assert.Equal(InvalidGrant, result[0].Error);
        Assert.Equal(expectedCode, result[0].Code);
        Assert.Equal(NotAllowed, context.FailureReason);
    }

    [Fact]
    public async Task ValidateAsync_PasswordGrantAllOrganizationsLocked_ClearsOrgIdAndReturnsEmpty()
    {
        // Arrange — the user's only organization is locked, so there's no other org to fall back to.
        AddMembership(OrgId, isLocked: true);

        var context = BuildContext(_user, GrantTypes.Password, OrgId);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
        Assert.Null(context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
    }

    [Fact]
    public async Task ValidateAsync_ActiveMembership_ReturnsEmpty()
    {
        // Arrange
        AddMembership(OrgId);

        var context = BuildContext(_user, GrantTypes.RefreshToken, OrgId);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ValidateAsync_ContactGloballyRejected_NoMembershipOverride_BlocksAccess()
    {
        // Arrange — the contact's global Status is Rejected and there is no per-organization override for
        // this org. The global Status is the effective default here, so sign-in must be blocked.
        _contact.Status = Rejected;
        AddMembership(OrgId);

        var context = BuildContext(_user, GrantTypes.Password, OrgId);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Single(result);
        Assert.Equal(InvalidGrant, result[0].Error);
        Assert.Equal("user_is_rejected_in_organization", result[0].Code);
        Assert.Equal(NotAllowed, context.FailureReason);
    }

    [Fact]
    public async Task ValidateAsync_ContactGloballyRejected_MembershipOverridesToApproved_DoesNotBlockAccess()
    {
        // Arrange — the contact's global Status is Rejected, but this specific organization's membership has
        // an explicit Approved override. The override wins, so sign-in must not be blocked.
        _contact.Status = Rejected;
        AddMembership(OrgId, Approved);

        var context = BuildContext(_user, GrantTypes.Password, OrgId);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ValidateAsync_AutoResolvedRejectedOrg_FallsBackToAccessibleOrgAndReturnsEmpty()
    {
        // Arrange — the member's current org is rejected, but they have another accessible one. Auto-detection
        // (no explicit org requested) already skips the rejected org and resolves straight to the accessible
        // one, so a fresh password sign-in must not be blocked entirely.
        _contact.CurrentOrganizationId = OrgId;
        AddMembership(OrgId, Rejected);
        AddMembership(OrgId2);

        var context = BuildContext(_user, GrantTypes.Password);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ValidateAsync_RefreshTokenGrant_SwitchToSecondAcceptedOrganization_ReturnsEmpty()
    {
        // Arrange
        AddMembership(OrgId);
        AddMembership(OrgId2, Approved);

        var context = BuildContext(_user, GrantTypes.RefreshToken, OrgId2);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ValidateAsync_RefreshTokenGrantExplicitSwitchToUnknownOrg_ReturnsInvalidOrganizationId()
    {
        // Arrange
        AddMembership(OrgId);

        var context = BuildContext(_user, GrantTypes.RefreshToken, "wrong-org");

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Single(result);
        Assert.Equal("invalid_organization_id", result[0].Code);
        Assert.Equal(NotAllowed, context.FailureReason);
    }

    [Fact]
    public async Task ValidateAsync_RefreshTokenGrantWithRejectedPrincipalOrg_FallsBackToAccessibleOrg()
    {
        // Arrange — plain token rotation sends no organization_id and keeps the org from the refresh token.
        AddMembership(OrgId, Rejected);
        AddMembership(OrgId2);

        var context = BuildContext(_user, GrantTypes.RefreshToken);
        context.Principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(OrganizationId, OrgId)], "test"));

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
        Assert.Equal(OrgId2, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
        Assert.Equal(OrgId2, context.Principal.FindFirstValue(OrganizationId));
        Assert.Null(context.FailureReason);
    }

    [Theory]
    [InlineData(GrantTypes.Password)]
    [InlineData(GrantTypes.AuthorizationCode)]
    [InlineData(PlatformGrantTypes.ExternalSignIn)]
    [InlineData(PlatformGrantTypes.Impersonate)]
    public async Task ValidateAsync_NonRefreshGrantWithRejectedOrg_FallsBackToAccessibleOrg(string grantType)
    {
        // Arrange — only an explicit refresh_token switch is deliberate, so a rejected org requested on sign-in is replaced with another accessible one.
        AddMembership(OrgId, Rejected);
        AddMembership(OrgId2);

        var context = BuildContext(_user, grantType, OrgId);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
        Assert.Equal(OrgId2, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
        Assert.Null(context.FailureReason);
    }

    [Theory]
    [InlineData(GrantTypes.Password)]
    [InlineData(GrantTypes.AuthorizationCode)]
    [InlineData(PlatformGrantTypes.ExternalSignIn)]
    [InlineData(PlatformGrantTypes.Impersonate)]
    public async Task ValidateAsync_NonRefreshGrantWithUnknownOrg_ReplacesWithAvailableOrg(string grantType)
    {
        // Arrange
        AddMembership(OrgId);

        var context = BuildContext(_user, grantType, "wrong-org");

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
        Assert.Equal(OrgId, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
        Assert.Null(context.FailureReason);
    }

    private void AddMembership(string organizationId, string status = null, bool isLocked = false)
    {
        _contact.Organizations.Add(organizationId);
        _memberships.Add(new OrganizationMembership { UserId = _user.Id, OrganizationId = organizationId, Status = status, IsLocked = isLocked });
    }

    private OrganizationIdRequestValidator GetValidator() =>
        new(_memberServiceMock.Object, _membershipServiceMock.Object);

    private static TokenRequestContext BuildContext(ApplicationUser user, string grantType, string requestedOrganizationId = null)
    {
        var request = new OpenIddictRequest { GrantType = grantType };

        if (requestedOrganizationId != null)
        {
            request.SetParameter(Parameters.OrganizationId, requestedOrganizationId);
        }

        return new TokenRequestContext { Request = request, User = user };
    }
}
