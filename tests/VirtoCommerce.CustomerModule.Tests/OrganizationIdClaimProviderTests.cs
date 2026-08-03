using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using OpenIddict.Abstractions;
using VirtoCommerce.CustomerModule.Core;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.OpenIddict;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.OpenIddict;
using Xunit;
using static VirtoCommerce.CustomerModule.Core.ModuleConstants.Security;

namespace VirtoCommerce.CustomerModule.Tests;

public class OrganizationIdClaimProviderTests
{
    private const string OrgId = "org1";
    private const string OrgId2 = "org2";
    private const string MemberId = "member1";
    private const string UserId = "user1";

    private readonly Mock<IMemberService> _memberServiceMock = new();
    private readonly Mock<IOrganizationMembershipSearchService> _membershipServiceMock = new();
    private readonly Mock<RoleManager<Role>> _roleManagerMock = new(new Mock<IRoleStore<Role>>().Object, null, null, null, null);

    public OrganizationIdClaimProviderTests()
    {
        _membershipServiceMock
            .Setup(s => s.SearchAsync(It.IsAny<OrganizationMembershipSearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync(new OrganizationMembershipSearchResult { Results = [] });

        _membershipServiceMock
            .Setup(s => s.GetRolesByUserAndOrgAsync(It.IsAny<string>(), It.IsAny<OrganizationMembership>()))
            .ReturnsAsync([]);
    }

    [Fact]
    public async Task SetClaimsAsync_AutoDetect_CurrentOrgIsInvited_SkipsItAndUsesAccessibleOrg()
    {
        //Arrange
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        _memberServiceMock.Setup(s => s.GetByIdAsync(MemberId, null, null))
            .ReturnsAsync(new Contact { Id = MemberId, Organizations = [OrgId, OrgId2], CurrentOrganizationId = OrgId });

        _membershipServiceMock
            .Setup(s => s.SearchAsync(
                It.Is<OrganizationMembershipSearchCriteria>(c => c.UserId == UserId && c.OrganizationIds != null && c.OrganizationIds.Contains(OrgId)),
                It.IsAny<bool>()))
            .ReturnsAsync(new OrganizationMembershipSearchResult
            {
                Results = [new OrganizationMembership { OrganizationId = OrgId, Status = ModuleConstants.MembershipStatuses.Invited }],
            });

        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var context = BuildContext(orgId: null, user: user);

        var provider = GetProvider();

        // Act
        await provider.SetClaimsAsync(principal, context);

        // Assert — must NOT be the still-Invited org1; must fall through to the accessible org2
        Assert.Equal(OrgId2, principal.FindFirstValue(Claims.OrganizationId));
    }

    [Fact]
    public async Task SetClaimsAsync_ExplicitOrganizationIdRequested_UsesItVerbatim()
    {
        var user = new ApplicationUser { Id = UserId, MemberId = MemberId };
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var context = BuildContext(OrgId, user: user);

        var provider = GetProvider();

        await provider.SetClaimsAsync(principal, context);

        Assert.Equal(OrgId, principal.FindFirstValue(Claims.OrganizationId));
    }

    private OrganizationIdClaimProvider GetProvider() =>
        new(_memberServiceMock.Object, _membershipServiceMock.Object, () => _roleManagerMock.Object);

    private static TokenRequestContext BuildContext(string orgId, ApplicationUser user)
    {
        var request = new OpenIddictRequest();
        if (orgId != null)
        {
            request.SetParameter(Parameters.OrganizationId, orgId);
        }

        return new TokenRequestContext { Request = request, User = user };
    }
}
