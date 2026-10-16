using System.Security.Claims;
using System.Threading.Tasks;
using Moq;
using OpenIddict.Abstractions;
using VirtoCommerce.CustomerModule.Core;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.OpenIddict;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.SignInLog;
using VirtoCommerce.Platform.Security.OpenIddict;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests;

public class OrganizationIdRequestValidatorTests
{
    private const string OrgId = "org1";
    private const string OrgId2 = "org2";
    private const string MemberId = "member1";
    private const string UserId = "user1";

    private readonly Mock<IMemberService> _memberServiceMock = new();
    private readonly Mock<IOrganizationMembershipSearchService> _membershipServiceMock = new();

    public OrganizationIdRequestValidatorTests()
    {
        _membershipServiceMock
            .Setup(s => s.SearchAsync(It.IsAny<OrganizationMembershipSearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync(new OrganizationMembershipSearchResult { Results = [] });
    }

    [Fact]
    public async Task ValidateAsync_NoOrganizationId_NoUser_ReturnsEmpty()
    {
        var result = await GetValidator().ValidateAsync(BuildContext(orgId: null));

        Assert.Empty(result);
    }

    [Fact]
    public async Task ValidateAsync_NoExplicitOrgId_AutoDetectsFromMember_ActiveMembership_ReturnsEmpty()
    {
        //Arrange
        var user = new ApplicationUser
        {
            Id = UserId,
            MemberId = MemberId
        };

        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact
            {
                Id = MemberId,
                Organizations = [OrgId]
            });

        SetupMembership(new OrganizationMembership { IsLocked = false });

        //Act
        var result = await GetValidator().ValidateAsync(BuildContext(orgId: null, user: user));

        //Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ValidateAsync_RefreshTokenGrant_NoExplicitOrgId_LockedInSoleOrg_IssuesTokenWithoutOrg()
    {
        //Arrange — the member's only organization is locked, so there is nothing to fall back to.
        var user = new ApplicationUser
        {
            Id = UserId,
            MemberId = MemberId
        };

        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact
            {
                Id = MemberId,
                Organizations = [OrgId]
            });

        SetupMembership(new OrganizationMembership { OrganizationId = OrgId, IsLocked = true, LockoutEnd = null });

        var context = BuildContext(orgId: null, grantType: OpenIddictConstants.GrantTypes.RefreshToken, user: user);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Empty(result);
        Assert.Null(context.Request.GetParameter(Parameters.OrganizationId));
    }

    [Fact]
    public async Task ValidateAsync_NoExplicitOrgId_MemberHasNoOrganizations_ReturnsEmpty()
    {
        //Arrange
        var user = new ApplicationUser
        {
            Id = UserId,
            MemberId = MemberId
        };

        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact
            {
                Id = MemberId,
                Organizations = []
            });

        //Act
        var result = await GetValidator().ValidateAsync(BuildContext(orgId: null, user: user));

        //Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ValidateAsync_RefreshTokenGrantExplicitSwitchToLockedOrg_ReturnsOrgError()
    {
        //Arrange
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId] });
        SetupMembership(new OrganizationMembership { IsLocked = true, LockoutEnd = null });

        var context = BuildContext(OrgId, grantType: OpenIddictConstants.GrantTypes.RefreshToken, user: user);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Single(result);
        Assert.Equal(OpenIddictConstants.Errors.InvalidGrant, result[0].Error);
        Assert.Equal(SignInFailureReason.NotAllowed, context.FailureReason);
    }

    [Fact]
    public async Task ValidateAsync_PasswordGrantAllOrganizationsLocked_ClearsOrgIdAndReturnsEmpty()
    {
        //Arrange — the user's only organization is locked, so there's no other org to fall back to.
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId] });
        SetupMembership(new OrganizationMembership { OrganizationId = OrgId, IsLocked = true, LockoutEnd = null });

        var context = BuildContext(OrgId, grantType: OpenIddictConstants.GrantTypes.Password, user: user);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Empty(result);
        Assert.Null(context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
    }

    [Fact]
    public async Task ValidateAsync_ActiveMembership_ReturnsEmpty()
    {
        //Arrange
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId] });
        SetupMembership(new OrganizationMembership { IsLocked = false });

        //Act
        var result = await GetValidator().ValidateAsync(BuildContext(OrgId, user: user));

        //Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ValidateAsync_InvalidOrgPasswordGrant_ReplacesWithDefaultAndReturnsEmpty()
    {
        //Arrange
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId] });

        var context = BuildContext("wrong-org", grantType: OpenIddictConstants.GrantTypes.Password, user: user);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Empty(result);
        Assert.Equal(OrgId, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
    }

    [Fact]
    public async Task ValidateAsync_PasswordGrantWithRejectedOrgParam_FallsBackToAccessibleOrgAndReturnsEmpty()
    {
        //Arrange — only an explicit refresh_token switch is deliberate, so an org requested on sign-in that is now rejected falls back to another accessible one.
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId, OrgId2] });

        SetupMembership(new OrganizationMembership { OrganizationId = OrgId, Status = ModuleConstants.MembershipStatuses.Rejected });

        var context = BuildContext(OrgId, grantType: OpenIddictConstants.GrantTypes.Password, user: user);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Empty(result);
        Assert.Equal(OrgId2, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
    }

    [Fact]
    public async Task ValidateAsync_RefreshTokenGrantExplicitSwitchToRejectedOrg_ReturnsOrgError()
    {
        //Arrange — refresh_token with organization_id (switchOrganization()) is a deliberate switch, so it is blocked instead of falling back to a different org.
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId, OrgId2] });

        SetupMembership(new OrganizationMembership { OrganizationId = OrgId, Status = ModuleConstants.MembershipStatuses.Rejected });

        var context = BuildContext(OrgId, grantType: OpenIddictConstants.GrantTypes.RefreshToken, user: user);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Single(result);
        Assert.Equal(OpenIddictConstants.Errors.InvalidGrant, result[0].Error);
        Assert.Equal(SignInFailureReason.NotAllowed, context.FailureReason);
    }

    [Fact]
    public async Task ValidateAsync_ContactGloballyRejected_NoMembershipOverride_BlocksAccess()
    {
        //Arrange — the contact's global Status is Rejected and there is no per-organization override for
        // this org. The global Status is the effective default here, so sign-in must be blocked.
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId], Status = ModuleConstants.MembershipStatuses.Rejected });

        var context = BuildContext(OrgId, grantType: OpenIddictConstants.GrantTypes.Password, user: user);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Single(result);
        Assert.Equal(OpenIddictConstants.Errors.InvalidGrant, result[0].Error);
    }

    [Fact]
    public async Task ValidateAsync_ContactGloballyRejected_MembershipOverridesToApproved_DoesNotBlockAccess()
    {
        //Arrange — the contact's global Status is Rejected, but this specific organization's membership has
        // an explicit Approved override. The override wins, so sign-in must not be blocked.
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };

        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId], Status = ModuleConstants.MembershipStatuses.Rejected });

        SetupMembership(new OrganizationMembership { OrganizationId = OrgId, Status = ModuleConstants.MembershipStatuses.Approved });

        var context = BuildContext(OrgId, grantType: OpenIddictConstants.GrantTypes.Password, user: user);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ValidateAsync_AutoResolvedRejectedOrg_FallsBackToAccessibleOrgAndReturnsEmpty()
    {
        //Arrange — the member's current org is rejected, but they have another accessible one. Auto-detection
        // (no explicit org requested) already skips the rejected org and resolves straight to the accessible
        // one, so a fresh password sign-in must not be blocked entirely.
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId, OrgId2], CurrentOrganizationId = OrgId });

        SetupMembership(new OrganizationMembership { OrganizationId = OrgId, Status = ModuleConstants.MembershipStatuses.Rejected });

        var context = BuildContext(orgId: null, grantType: OpenIddictConstants.GrantTypes.Password, user: user);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ValidateAsync_RefreshTokenGrant_SwitchToSecondAcceptedOrganization_ReturnsEmpty()
    {
        //Arrange
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId, OrgId2] });

        _membershipServiceMock
            .Setup(s => s.SearchAsync(
                It.Is<OrganizationMembershipSearchCriteria>(c => c.UserId == UserId && c.OrganizationId == OrgId2),
                It.IsAny<bool>()))
            .ReturnsAsync(new OrganizationMembershipSearchResult
            {
                Results = [new OrganizationMembership { OrganizationId = OrgId2, Status = ModuleConstants.MembershipStatuses.Approved }],
            });

        var context = BuildContext(OrgId2, grantType: OpenIddictConstants.GrantTypes.RefreshToken, user: user);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ValidateAsync_RefreshTokenGrantExplicitSwitchToUnknownOrg_ReturnsInvalidOrganizationId()
    {
        //Arrange
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId] });

        var context = BuildContext("wrong-org", grantType: OpenIddictConstants.GrantTypes.RefreshToken, user: user);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Single(result);
        Assert.Equal("invalid_organization_id", result[0].Code);
        Assert.Equal(SignInFailureReason.NotAllowed, context.FailureReason);
    }

    [Fact]
    public async Task ValidateAsync_RefreshTokenGrantWithRejectedPrincipalOrg_FallsBackToAccessibleOrg()
    {
        //Arrange — plain token rotation sends no organization_id and keeps the org from the refresh token.
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId, OrgId2] });

        SetupMembership(new OrganizationMembership { OrganizationId = OrgId, Status = ModuleConstants.MembershipStatuses.Rejected });

        var context = BuildContext(orgId: null, grantType: OpenIddictConstants.GrantTypes.RefreshToken, user: user);
        context.Principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ModuleConstants.Security.Claims.OrganizationId, OrgId)], "test"));

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Empty(result);
        Assert.Equal(OrgId2, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
        Assert.Null(context.FailureReason);
    }

    [Theory]
    [InlineData(OpenIddictConstants.GrantTypes.Password)]
    [InlineData(OpenIddictConstants.GrantTypes.AuthorizationCode)]
    [InlineData(PlatformConstants.Security.GrantTypes.ExternalSignIn)]
    [InlineData(PlatformConstants.Security.GrantTypes.Impersonate)]
    [InlineData("passkey")]
    public async Task ValidateAsync_NonRefreshGrantWithRejectedOrg_FallsBackToAccessibleOrg(string grantType)
    {
        //Arrange
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId, OrgId2] });

        SetupMembership(new OrganizationMembership { OrganizationId = OrgId, Status = ModuleConstants.MembershipStatuses.Rejected });

        var context = BuildContext(OrgId, grantType: grantType, user: user);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Empty(result);
        Assert.Equal(OrgId2, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
        Assert.Null(context.FailureReason);
    }

    [Theory]
    [InlineData(OpenIddictConstants.GrantTypes.Password)]
    [InlineData(OpenIddictConstants.GrantTypes.AuthorizationCode)]
    [InlineData(PlatformConstants.Security.GrantTypes.ExternalSignIn)]
    [InlineData(PlatformConstants.Security.GrantTypes.Impersonate)]
    [InlineData("passkey")]
    public async Task ValidateAsync_NonRefreshGrantWithUnknownOrg_ReplacesWithAvailableOrg(string grantType)
    {
        //Arrange
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId] });

        var context = BuildContext("wrong-org", grantType: grantType, user: user);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Empty(result);
        Assert.Equal(OrgId, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
        Assert.Null(context.FailureReason);
    }

    [Fact]
    public async Task ValidateAsync_PasswordGrantWithWrongPassword_ReturnsNoOrganizationError()
    {
        //Arrange — the member is rejected in their only org, which would otherwise return a status error.
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId], Status = ModuleConstants.MembershipStatuses.Rejected });

        var context = BuildContext(OrgId, grantType: OpenIddictConstants.GrantTypes.Password, user: user);
        context.SignInResult = Microsoft.AspNetCore.Identity.SignInResult.Failed;

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Empty(result);
        Assert.Null(context.FailureReason);
    }

    private void SetupMembership(OrganizationMembership membership) =>
        _membershipServiceMock
            .Setup(s => s.SearchAsync(
                It.Is<OrganizationMembershipSearchCriteria>(c =>
                    c.UserId == UserId &&
                    (c.OrganizationId == OrgId || (c.OrganizationIds != null && c.OrganizationIds.Contains(OrgId)))),
                It.IsAny<bool>()))
            .ReturnsAsync(new OrganizationMembershipSearchResult { Results = [membership] });

    private OrganizationIdRequestValidator GetValidator() =>
        new(_memberServiceMock.Object, _membershipServiceMock.Object);

    private static TokenRequestContext BuildContext(
        string orgId,
        string grantType = OpenIddictConstants.GrantTypes.RefreshToken,
        ApplicationUser user = null)
    {
        var request = new OpenIddictRequest { GrantType = grantType };
        if (orgId != null)
        {
            request.SetParameter(Parameters.OrganizationId, orgId);
        }

        return new TokenRequestContext { Request = request, User = user };
    }
}
