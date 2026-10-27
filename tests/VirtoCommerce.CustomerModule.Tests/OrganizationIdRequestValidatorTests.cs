using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using OpenIddict.Abstractions;
using VirtoCommerce.CustomerModule.Core;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.OpenIddict;
using VirtoCommerce.Platform.Core;
using VirtoCommerce.Platform.Core.Security;
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
    public async Task ValidateAsync_NoExplicitOrgId_AutoDetectsFromMember_LockedInOrg_ReturnsOrgError()
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

        SetupMembership(new OrganizationMembership { IsLocked = true, LockoutEnd = null });

        //Act
        var result = await GetValidator().ValidateAsync(BuildContext(orgId: null, user: user));

        //Assert
        Assert.Single(result);
        Assert.Equal(OpenIddictConstants.Errors.InvalidGrant, result[0].Error);
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
    public async Task ValidateAsync_UserGloballyLocked_ReturnsGlobalError()
    {
        //Arrange
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId] });

        var user = new ApplicationUser
        {
            Id = UserId,
            MemberId = MemberId,
            LockoutEnabled = true,
            LockoutEnd = DateTimeOffset.UtcNow.AddDays(1)
        };

        //Act
        var result = await GetValidator().ValidateAsync(BuildContext(OrgId, user: user));

        //Assert
        Assert.Single(result);
        Assert.Equal(OpenIddictConstants.Errors.InvalidGrant, result[0].Error);
    }

    [Fact]
    public async Task ValidateAsync_UserTemporarilyLocked_ReturnsTemporaryLockoutError()
    {
        //Arrange — org member with an active TEMPORARY lock (future end, NOT DateTime.MaxValue),
        // e.g. after exceeding the failed-attempt threshold. VCST-5374: this must surface the
        // temporary lockout code, not the permanent one.
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId] });

        var user = new ApplicationUser
        {
            Id = UserId,
            MemberId = MemberId,
            LockoutEnabled = true,
            LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(15)
        };

        //Act
        var result = await GetValidator().ValidateAsync(BuildContext(OrgId, user: user));

        //Assert
        Assert.Single(result);
        Assert.Equal(OpenIddictConstants.Errors.InvalidGrant, result[0].Error);
        Assert.NotEqual("user_is_locked_out", result[0].Code);
        Assert.Equal("user_is_temporary_locked_out", result[0].Code);
    }

    [Fact]
    public async Task ValidateAsync_UserPermanentlyLocked_ReturnsGlobalLockoutError()
    {
        //Arrange — org member with a PERMANENT lock (LockoutEnd == DateTime.MaxValue).
        // Regression guard: the permanent code must still be returned.
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId] });

        var user = new ApplicationUser
        {
            Id = UserId,
            MemberId = MemberId,
            LockoutEnabled = true,
            LockoutEnd = DateTime.MaxValue.ToUniversalTime()
        };

        //Act
        var result = await GetValidator().ValidateAsync(BuildContext(OrgId, user: user));

        //Assert
        Assert.Single(result);
        Assert.Equal(OpenIddictConstants.Errors.InvalidGrant, result[0].Error);
        Assert.Equal("user_is_locked_out", result[0].Code);
    }

    [Fact]
    public async Task ValidateAsync_UserLockedInOrg_ReturnsOrgError()
    {
        //Arrange
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId] });
        SetupMembership(new OrganizationMembership { IsLocked = true, LockoutEnd = null });

        //Act
        var result = await GetValidator().ValidateAsync(BuildContext(OrgId, user: user));

        //Assert
        Assert.Single(result);
        Assert.Equal(OpenIddictConstants.Errors.InvalidGrant, result[0].Error);
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

    [Theory]
    [MemberData(nameof(CredentialGrantTypeData))]
    public async Task ValidateAsync_CredentialSignInWithUnavailableOrg_FallsBackToAccessibleOrgAndReturnsEmpty(string grantType)
    {
        //Arrange — credential sign-ins resend the last-used org (useAuth.ts), which is never a deliberate choice,
        // so an org the user is no longer in must fall back instead of blocking sign-in.
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId2] });

        var context = BuildContext(OrgId, grantType, user, SignInResult.Success);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Empty(result);
        Assert.Equal(OrgId2, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
    }

    [Theory]
    [MemberData(nameof(CredentialGrantTypeData))]
    public async Task ValidateAsync_CredentialSignInWithLockedOrg_FallsBackToAccessibleOrgAndReturnsEmpty(string grantType)
    {
        //Arrange
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId, OrgId2] });

        SetupMembership(new OrganizationMembership { OrganizationId = OrgId, IsLocked = true, LockoutEnd = null });

        var context = BuildContext(OrgId, grantType, user, SignInResult.Success);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Empty(result);
        Assert.Equal(OrgId2, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
    }

    [Theory]
    [MemberData(nameof(CredentialGrantTypeBlockingStatusData))]
    public async Task ValidateAsync_CredentialSignInWithBlockedOrg_FallsBackToAccessibleOrgAndReturnsEmpty(string grantType, string status)
    {
        //Arrange
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId, OrgId2] });

        SetupMembership(new OrganizationMembership { OrganizationId = OrgId, Status = status });

        var context = BuildContext(OrgId, grantType, user, SignInResult.Success);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Empty(result);
        Assert.Equal(OrgId2, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
    }

    [Theory]
    [MemberData(nameof(CredentialGrantTypeData))]
    public async Task ValidateAsync_CredentialSignInWithUnavailableOrg_NoAccessibleOrgLeft_ClearsOrgIdAndReturnsEmpty(string grantType)
    {
        //Arrange
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [] });

        var context = BuildContext(OrgId, grantType, user, SignInResult.Success);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Empty(result);
        Assert.Null(context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
    }

    [Theory]
    [MemberData(nameof(CredentialGrantTypeData))]
    public async Task ValidateAsync_CredentialSignInWithLockedOrg_NoAccessibleOrgLeft_ClearsOrgIdAndReturnsEmpty(string grantType)
    {
        //Arrange — the user's only organization is locked, so there's no other org to fall back to.
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId] });
        SetupMembership(new OrganizationMembership { OrganizationId = OrgId, IsLocked = true, LockoutEnd = null });

        var context = BuildContext(OrgId, grantType, user, SignInResult.Success);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Empty(result);
        Assert.Null(context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
    }

    [Theory]
    [MemberData(nameof(CredentialGrantTypeBlockingStatusData))]
    public async Task ValidateAsync_CredentialSignInWithBlockedOrg_NoAccessibleOrgLeft_ReturnsOrgError(string grantType, string status)
    {
        //Arrange
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId] });
        SetupMembership(new OrganizationMembership { OrganizationId = OrgId, Status = status });

        var context = BuildContext(OrgId, grantType, user, SignInResult.Success);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Single(result);
        Assert.Equal(OpenIddictConstants.Errors.InvalidGrant, result[0].Error);
        Assert.Equal(StatusErrorCodes[status], result[0].Code);
    }

    [Theory]
    [MemberData(nameof(SignInResultData))]
    public async Task ValidateAsync_AnySignInResult_FallsBackToAccessibleOrgAndReturnsEmpty(string signInResult)
    {
        //Arrange — the validators also run after a failed password check, so the fallback must not depend
        // on whether the sign-in succeeded, only on SignInResult being set.
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId2] });

        var context = BuildContext(OrgId, OpenIddictConstants.GrantTypes.Password, user, SignInResults[signInResult]);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Empty(result);
        Assert.Equal(OrgId2, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
    }

    [Fact]
    public async Task ValidateAsync_PasswordGrantWithoutSignInResult_ReturnsOrgError()
    {
        //Arrange — the fallback follows SignInResult, not the grant name.
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId2] });

        var context = BuildContext(OrgId, OpenIddictConstants.GrantTypes.Password, user);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Single(result);
        Assert.Equal(OpenIddictConstants.Errors.InvalidGrant, result[0].Error);
    }

    [Fact]
    public async Task ValidateAsync_CustomGrantSettingSignInResult_FallsBackToAccessibleOrgAndReturnsEmpty()
    {
        //Arrange — the fallback follows TokenRequestContext.SignInResult, not grant names,
        // so a project's own credential grant gets it too.
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId2] });

        var context = BuildContext(OrgId, "custom_credential_grant", user, SignInResult.Success);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Empty(result);
        Assert.Equal(OrgId2, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
    }

    [Theory]
    [MemberData(nameof(NonCredentialGrantTypeData))]
    public async Task ValidateAsync_NonCredentialGrantWithUnavailableOrg_ReturnsOrgError(string grantType)
    {
        //Arrange — an explicit org switch (refresh_token grant, switchOrganization()) IS a deliberate choice,
        // so it must be blocked rather than silently falling back to a different org.
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId2] });

        var context = BuildContext(OrgId, grantType, user);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Single(result);
        Assert.Equal(OpenIddictConstants.Errors.InvalidGrant, result[0].Error);
    }

    [Theory]
    [MemberData(nameof(NonCredentialGrantTypeData))]
    public async Task ValidateAsync_NonCredentialGrantWithLockedOrg_ReturnsOrgError(string grantType)
    {
        //Arrange
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId, OrgId2] });

        SetupMembership(new OrganizationMembership { OrganizationId = OrgId, IsLocked = true, LockoutEnd = null });

        var context = BuildContext(OrgId, grantType, user);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Single(result);
        Assert.Equal(OpenIddictConstants.Errors.InvalidGrant, result[0].Error);
    }

    [Theory]
    [MemberData(nameof(NonCredentialGrantTypeData))]
    public async Task ValidateAsync_NonCredentialGrantWithRejectedOrg_ReturnsOrgError(string grantType)
    {
        //Arrange
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId, OrgId2] });

        SetupMembership(new OrganizationMembership { OrganizationId = OrgId, Status = ModuleConstants.MembershipStatuses.Rejected });

        var context = BuildContext(OrgId, grantType, user);

        //Act
        var result = await GetValidator().ValidateAsync(context);

        //Assert
        Assert.Single(result);
        Assert.Equal(OpenIddictConstants.Errors.InvalidGrant, result[0].Error);
    }

    [Fact]
    public async Task ValidateAsync_ContactGloballyRejected_NoMembershipOverride_BlocksAccess()
    {
        //Arrange — the contact's global Status is Rejected and there is no per-organization override for
        // this org. The global Status is the effective default here, so sign-in must be blocked.
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId], Status = ModuleConstants.MembershipStatuses.Rejected });

        var context = BuildContext(OrgId, OpenIddictConstants.GrantTypes.Password, user, SignInResult.Success);

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

        var context = BuildContext(OrgId, OpenIddictConstants.GrantTypes.Password, user, SignInResult.Success);

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

        var context = BuildContext(orgId: null, OpenIddictConstants.GrantTypes.Password, user, SignInResult.Success);

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

    // Credential grants that set TokenRequestContext.SignInResult: platform password, vc-module-otp, vc-module-mfa.
    // Row labels only: the credential theories pass SignInResult explicitly.
    private static readonly string[] CredentialGrantTypes = [OpenIddictConstants.GrantTypes.Password, "otp_email", "mfa"];

    private static readonly string[] NonCredentialGrantTypes =
    [
        OpenIddictConstants.GrantTypes.RefreshToken,
        OpenIddictConstants.GrantTypes.AuthorizationCode,
        PlatformConstants.Security.GrantTypes.Impersonate,
        PlatformConstants.Security.GrantTypes.ExternalSignIn,
    ];

    public static TheoryData<string> CredentialGrantTypeData => new(CredentialGrantTypes);

    public static TheoryData<string> NonCredentialGrantTypeData => new(NonCredentialGrantTypes);

    public static MatrixTheoryData<string, string> CredentialGrantTypeBlockingStatusData =>
        new(CredentialGrantTypes, ModuleConstants.MembershipStatuses.BlockingStatuses);

    private static readonly Dictionary<string, SignInResult> SignInResults = new()
    {
        [nameof(SignInResult.Success)] = SignInResult.Success,
        [nameof(SignInResult.Failed)] = SignInResult.Failed,
        [nameof(SignInResult.LockedOut)] = SignInResult.LockedOut,
        [nameof(SignInResult.NotAllowed)] = SignInResult.NotAllowed,
    };

    public static TheoryData<string> SignInResultData => new(SignInResults.Keys);

    private static readonly Dictionary<string, string> StatusErrorCodes = new()
    {
        [ModuleConstants.MembershipStatuses.Invited] = "user_invitation_pending_in_organization",
        [ModuleConstants.MembershipStatuses.Rejected] = "user_is_rejected_in_organization",
        [ModuleConstants.MembershipStatuses.Deleted] = "user_is_removed_from_organization",
    };

    private static TokenRequestContext BuildContext(
        string orgId,
        string grantType = OpenIddictConstants.GrantTypes.RefreshToken,
        ApplicationUser user = null,
        SignInResult signInResult = null)
    {
        var request = new OpenIddictRequest { GrantType = grantType };
        if (orgId != null)
        {
            request.SetParameter(Parameters.OrganizationId, orgId);
        }

        return new TokenRequestContext
        {
            Request = request,
            User = user,
            SignInResult = signInResult,
        };
    }
}
