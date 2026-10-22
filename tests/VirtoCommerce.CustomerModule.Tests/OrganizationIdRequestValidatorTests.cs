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
    private const string UnknownOrgId = "wrong-org";
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
    public async Task NoUser_ReturnsEmpty()
    {
        // Arrange
        var context = BuildContext(user: null, GrantTypes.RefreshToken);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
        Assert.Null(context.FailureReason);
    }

    [Fact]
    public async Task TokenRotationWithoutTokenOrg_AccessibleContactOrg_ReturnsEmpty()
    {
        // Arrange
        AddMembership(OrgId);

        var context = BuildContext(_user, GrantTypes.RefreshToken);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
        Assert.Null(context.Request.GetParameter(Parameters.OrganizationId));
        Assert.Null(context.FailureReason);
    }

    [Fact]
    public async Task TokenRotationWithLockedOrg_NoAccessibleOrgLeft_ClearsOrgId()
    {
        // Arrange
        AddMembership(OrgId, isLocked: true);

        var context = BuildContext(_user, GrantTypes.RefreshToken);
        context.Principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(OrganizationId, OrgId)], "test"));

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
        Assert.Null(context.Request.GetParameter(Parameters.OrganizationId));
        Assert.Null(context.Principal.FindFirstValue(OrganizationId));
        Assert.Null(context.FailureReason);
    }

    [Fact]
    public async Task TokenRotationWithoutTokenOrg_NoContactOrgs_ReturnsEmpty()
    {
        // Arrange
        var context = BuildContext(_user, GrantTypes.RefreshToken);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
        Assert.Null(context.FailureReason);
    }

    [Theory]
    [InlineData(null, true, "user_is_locked_in_organization")]
    [InlineData(Rejected, false, "user_is_rejected_in_organization")]
    public async Task SwitchToUnavailableOrg_ReturnsOrgError(string status, bool isLocked, string expectedCode)
    {
        // Arrange
        AddMembership(OrgId, status, isLocked);
        AddMembership(OrgId2);

        var context = BuildContext(_user, GrantTypes.RefreshToken, OrgId);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Single(result);
        Assert.Equal(InvalidGrant, result[0].Error);
        Assert.Equal(expectedCode, result[0].Code);
        Assert.Contains(OrgId, result[0].ErrorDescription);
        Assert.Equal(OrgId, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
        Assert.Equal(NotAllowed, context.FailureReason);
    }

    [Fact]
    public async Task SwitchToAccessibleOrg_ReturnsEmpty()
    {
        // Arrange
        AddMembership(OrgId);

        var context = BuildContext(_user, GrantTypes.RefreshToken, OrgId);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
        Assert.Equal(OrgId, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
        Assert.Null(context.FailureReason);
    }

    [Fact]
    public async Task SignInWithRejectedContact_NoMembershipOverride_ReturnsOrgError()
    {
        // Arrange
        _contact.Status = Rejected;
        AddMembership(OrgId);

        var context = BuildContext(_user, GrantTypes.Password, OrgId);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Single(result);
        Assert.Equal(InvalidGrant, result[0].Error);
        Assert.Equal("user_is_rejected_in_organization", result[0].Code);
        Assert.Contains(OrgId, result[0].ErrorDescription);
        Assert.Equal(OrgId, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
        Assert.Equal(NotAllowed, context.FailureReason);
    }

    [Fact]
    public async Task SignInWithRejectedContact_ApprovedMembership_ReturnsEmpty()
    {
        // Arrange
        _contact.Status = Rejected;
        AddMembership(OrgId, Approved);

        var context = BuildContext(_user, GrantTypes.Password, OrgId);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
        Assert.Equal(OrgId, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
        Assert.Null(context.FailureReason);
    }

    [Fact]
    public async Task SignInWithoutOrgId_RejectedCurrentOrg_ReturnsEmpty()
    {
        // Arrange
        _contact.CurrentOrganizationId = OrgId;
        AddMembership(OrgId, Rejected);
        AddMembership(OrgId2);

        var context = BuildContext(_user, GrantTypes.Password);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
        Assert.Null(context.Request.GetParameter(Parameters.OrganizationId));
        Assert.Null(context.FailureReason);
    }

    [Fact]
    public async Task SwitchToSecondApprovedOrg_ReturnsEmpty()
    {
        // Arrange
        AddMembership(OrgId);
        AddMembership(OrgId2, Approved);

        var context = BuildContext(_user, GrantTypes.RefreshToken, OrgId2);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
        Assert.Equal(OrgId2, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
        Assert.Null(context.FailureReason);
    }

    [Fact]
    public async Task SwitchToUnknownOrg_ReturnsInvalidOrganizationId()
    {
        // Arrange
        AddMembership(OrgId);

        var context = BuildContext(_user, GrantTypes.RefreshToken, UnknownOrgId);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Single(result);
        Assert.Equal(InvalidGrant, result[0].Error);
        Assert.Equal("invalid_organization_id", result[0].Code);
        Assert.Contains(UnknownOrgId, result[0].ErrorDescription);
        Assert.Equal(UnknownOrgId, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
        Assert.Equal(NotAllowed, context.FailureReason);
    }

    [Fact]
    public async Task TokenRotationWithRejectedOrg_FallsBackToAccessibleOrg()
    {
        // Arrange
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
    [MemberData(nameof(FallbackGrantTypeBlockingStatusData))]
    public async Task SignInWithBlockedOrg_FallsBackToAccessibleOrg(string grantType, string status)
    {
        // Arrange
        AddMembership(OrgId, status);
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
    [MemberData(nameof(FallbackGrantTypeData))]
    public async Task SignInWithLockedOrg_FallsBackToAccessibleOrg(string grantType)
    {
        // Arrange
        AddMembership(OrgId, isLocked: true);
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
    [MemberData(nameof(FallbackGrantTypeData))]
    public async Task SignInWithUnknownOrg_FallsBackToAccessibleOrg(string grantType)
    {
        // Arrange
        AddMembership(OrgId);

        var context = BuildContext(_user, grantType, UnknownOrgId);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
        Assert.Equal(OrgId, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
        Assert.Null(context.FailureReason);
    }

    [Theory]
    [MemberData(nameof(FallbackGrantTypeData))]
    public async Task SignInWithUnknownOrg_NoAccessibleOrgLeft_ClearsOrgId(string grantType)
    {
        // Arrange
        var context = BuildContext(_user, grantType, OrgId);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
        Assert.Null(context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
        Assert.Null(context.FailureReason);
    }

    [Theory]
    [MemberData(nameof(FallbackGrantTypeData))]
    public async Task SignInWithLockedOrg_NoAccessibleOrgLeft_ClearsOrgId(string grantType)
    {
        // Arrange
        AddMembership(OrgId, isLocked: true);

        var context = BuildContext(_user, grantType, OrgId);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Empty(result);
        Assert.Null(context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
        Assert.Null(context.FailureReason);
    }

    [Theory]
    [MemberData(nameof(FallbackGrantTypeBlockingStatusData))]
    public async Task SignInWithBlockedOrg_NoAccessibleOrgLeft_ReturnsOrgError(string grantType, string status)
    {
        // Arrange
        AddMembership(OrgId, status);

        var context = BuildContext(_user, grantType, OrgId);

        // Act
        var result = await GetValidator().ValidateAsync(context);

        // Assert
        Assert.Single(result);
        Assert.Equal(InvalidGrant, result[0].Error);
        Assert.Equal(StatusErrorCodes[status], result[0].Code);
        Assert.Contains(OrgId, result[0].ErrorDescription);
        Assert.Equal(OrgId, context.Request.GetParameter(Parameters.OrganizationId)?.ToString());
        Assert.Equal(NotAllowed, context.FailureReason);
    }

    private void AddMembership(string organizationId, string status = null, bool isLocked = false)
    {
        _contact.Organizations.Add(organizationId);
        _memberships.Add(new OrganizationMembership { UserId = _user.Id, OrganizationId = organizationId, Status = status, IsLocked = isLocked });
    }

    private static TokenRequestContext BuildContext(ApplicationUser user, string grantType, string requestedOrganizationId = null)
    {
        var request = new OpenIddictRequest { GrantType = grantType };

        if (requestedOrganizationId != null)
        {
            request.SetParameter(Parameters.OrganizationId, requestedOrganizationId);
        }

        return new TokenRequestContext { Request = request, User = user };
    }

    private OrganizationIdRequestValidator GetValidator() =>
        new(_memberServiceMock.Object, _membershipServiceMock.Object);

    private static readonly string[] FallbackGrantTypes =
    [
        GrantTypes.Password,
        GrantTypes.AuthorizationCode,
        PlatformGrantTypes.ExternalSignIn,
        PlatformGrantTypes.Impersonate,
        "not_refresh_token",
    ];

    public static TheoryData<string> FallbackGrantTypeData => new(FallbackGrantTypes);

    public static MatrixTheoryData<string, string> FallbackGrantTypeBlockingStatusData => new(FallbackGrantTypes, BlockingStatuses);

    private static readonly Dictionary<string, string> StatusErrorCodes = new()
    {
        [Invited] = "user_invitation_pending_in_organization",
        [Rejected] = "user_is_rejected_in_organization",
        [Deleted] = "user_is_removed_from_organization",
    };
}
