using System;
using System.Threading.Tasks;
using Moq;
using OpenIddict.Abstractions;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.OpenIddict;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.OpenIddict;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests;

public class OrganizationIdRequestValidatorTests
{
    private const string OrgId = "org1";
    private const string MemberId = "member1";
    private const string UserId = "user1";

    private readonly Mock<IMemberService> _memberServiceMock = new();
    private readonly Mock<IOrganizationMembershipService> _membershipServiceMock = new();

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

        _membershipServiceMock.Setup(s => s.GetByUserAndOrgAsync(UserId, OrgId))
            .ReturnsAsync(new OrganizationMembership
            {
                IsLocked = false
            });

        //Act
        var result = await GetValidator().ValidateAsync(BuildContext(orgId: null, user: user));

        //Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ValidateAsync_NoExplicitOrgId_AutoDetectsFromMember_LockedInOrg_ReturnsOrgError()
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

        _membershipServiceMock.Setup(s => s.GetByUserAndOrgAsync(UserId, OrgId))
            .ReturnsAsync(new OrganizationMembership
            {
                IsLocked = true,
                LockoutEnd = null
            });

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
        _membershipServiceMock.Setup(s => s.GetByUserAndOrgAsync(UserId, OrgId))
            .ReturnsAsync(new OrganizationMembership { IsLocked = true, LockoutEnd = null });

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
        _membershipServiceMock.Setup(s => s.GetByUserAndOrgAsync(UserId, OrgId))
            .ReturnsAsync(new OrganizationMembership { IsLocked = false });

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
