using System.Linq;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.SearchModule.Core.Model;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests;

[Trait("Category", "CI")]
public class MemberDocumentBuilderTests
{
    private readonly Mock<IMemberService> _memberServiceMock = new();
    private readonly Mock<IOrganizationMembershipService> _membershipServiceMock = new();

    [Fact]
    public async Task IndexOrganizationMembershipRolesAsync_NoSecurityAccounts_SkipsAllServiceCalls()
    {
        //Arrange
        var contact = new Contact { Id = "c1", SecurityAccounts = [] };
        var doc = new IndexDocument("c1");

        //Act
        await GetBuilder().IndexRolesAsync(doc, contact);

        //Assert
        _membershipServiceMock.Verify(
            s => s.SearchAsync(It.IsAny<OrganizationMembershipSearchCriteria>(), It.IsAny<bool>()),
            Times.Never);

        _memberServiceMock.Verify(
            s => s.GetByIdsAsync(It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string[]>()),
            Times.Never);
    }

    [Fact]
    public async Task IndexOrganizationMembershipRolesAsync_NoMemberships_SkipsOrgFetch()
    {
        //Arrange
        var contact = new Contact
        {
            Id = "c1",
            SecurityAccounts = [new ApplicationUser { Id = "user-1" }]
        };

        _membershipServiceMock
            .Setup(s => s.SearchAsync(It.IsAny<OrganizationMembershipSearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync(new OrganizationMembershipSearchResult { Results = [] });

        var doc = new IndexDocument("c1");

        //Act
        await GetBuilder().IndexRolesAsync(doc, contact);

        //Assert
        _memberServiceMock.Verify(
            s => s.GetByIdsAsync(It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<string[]>()),
            Times.Never);
    }

    [Fact]
    public async Task IndexOrganizationMembershipRolesAsync_MembershipRoles_IndexedOnDocument()
    {
        //Arrange
        var contact = new Contact
        {
            Id = "c1",
            SecurityAccounts = [new ApplicationUser { Id = "user-1", Roles = [] }]
        };

        _membershipServiceMock
            .Setup(s => s.SearchAsync(
                It.Is<OrganizationMembershipSearchCriteria>(c => c.UserId == "user-1"),
                It.IsAny<bool>()))
            .ReturnsAsync(new OrganizationMembershipSearchResult
            {
                Results =
                [
                    new OrganizationMembership
                    {
                        OrganizationId = "org-1",
                        Roles = [new OrganizationMembershipRole { RoleId = "membership-role-1", RoleName = "Buyer" }]
                    }
                ]
            });

        _memberServiceMock
            .Setup(s => s.GetByIdsAsync(It.IsAny<string[]>(), MemberResponseGroup.WithRoles.ToString(), It.IsAny<string[]>()))
            .ReturnsAsync([]);

        var doc = new IndexDocument("c1");

        //Act
        await GetBuilder().IndexRolesAsync(doc, contact);

        //Assert
        var roleIdField = doc.Fields.FirstOrDefault(f => f.Name == "RoleId");

        Assert.NotNull(roleIdField);
        Assert.Contains("membership-role-1", roleIdField.Values.OfType<string>());

        var roleField = doc.Fields.FirstOrDefault(f => f.Name == "Role");

        Assert.NotNull(roleField);
        Assert.Contains("Buyer", roleField.Values.OfType<string>());
    }

    [Fact]
    public async Task IndexOrganizationMembershipRolesAsync_OrgRoles_IndexedOnDocument()
    {
        //Arrange
        var contact = new Contact
        {
            Id = "c1",
            SecurityAccounts = [new ApplicationUser { Id = "user-1", Roles = [] }]
        };

        _membershipServiceMock
            .Setup(s => s.SearchAsync(It.IsAny<OrganizationMembershipSearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync(new OrganizationMembershipSearchResult
            {
                Results = [new OrganizationMembership { OrganizationId = "org-1", Roles = [] }]
            });

        _memberServiceMock
            .Setup(s => s.GetByIdsAsync(It.IsAny<string[]>(), MemberResponseGroup.WithRoles.ToString(), It.IsAny<string[]>()))
            .ReturnsAsync(
            [
                new Organization
                {
                    Id = "org-1",
                    Roles = [new OrganizationRole { RoleId = "org-role-1", RoleName = "Manager" }]
                }
            ]);

        var doc = new IndexDocument("c1");

        //Act
        await GetBuilder().IndexRolesAsync(doc, contact);

        //Assert
        var roleIdField = doc.Fields.FirstOrDefault(f => f.Name == "RoleId");

        Assert.NotNull(roleIdField);
        Assert.Contains("org-role-1", roleIdField.Values.OfType<string>());

        var roleField = doc.Fields.FirstOrDefault(f => f.Name == "Role");

        Assert.NotNull(roleField);
        Assert.Contains("Manager", roleField.Values.OfType<string>());
    }

    [Fact]
    public async Task IndexOrganizationMembershipRolesAsync_RoleAlreadyInSecurityAccount_NotIndexedAgain()
    {
        // Roles already in existingRoleIds (from SecurityAccounts) must not be added twice.
        //Arrange
        var contact = new Contact
        {
            Id = "c1",
            SecurityAccounts =
            [
                new ApplicationUser
                {
                    Id = "user-1",
                    Roles = [new Role { Id = "shared-role", NormalizedName = "Admin" }]
                }
            ]
        };

        _membershipServiceMock
            .Setup(s => s.SearchAsync(It.IsAny<OrganizationMembershipSearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync(new OrganizationMembershipSearchResult
            {
                Results =
                [
                    new OrganizationMembership
                    {
                        OrganizationId = "org-1",
                        Roles = [new OrganizationMembershipRole { RoleId = "shared-role", RoleName = "Admin" }]
                    }
                ]
            });

        _memberServiceMock
            .Setup(s => s.GetByIdsAsync(It.IsAny<string[]>(), MemberResponseGroup.WithRoles.ToString(), It.IsAny<string[]>()))
            .ReturnsAsync([]);

        var doc = new IndexDocument("c1");

        //Act
        await GetBuilder().IndexRolesAsync(doc, contact);

        //Assert — "shared-role" not added (already in existingRoleIds from security accounts)
        Assert.Null(doc.Fields.FirstOrDefault(f => f.Name == "RoleId"));
    }

    [Fact]
    public async Task IndexOrganizationMembershipRolesAsync_OrgRoleAlreadyInMembership_NotIndexedTwice()
    {
        // Org-level role duplicating a membership role must not be indexed a second time.
        //Arrange
        var contact = new Contact
        {
            Id = "c1",
            SecurityAccounts = [new ApplicationUser { Id = "user-1", Roles = [] }]
        };

        _membershipServiceMock
            .Setup(s => s.SearchAsync(It.IsAny<OrganizationMembershipSearchCriteria>(), It.IsAny<bool>()))
            .ReturnsAsync(new OrganizationMembershipSearchResult
            {
                Results =
                [
                    new OrganizationMembership
                    {
                        OrganizationId = "org-1",
                        Roles = [new OrganizationMembershipRole { RoleId = "role1", RoleName = "Admin" }]
                    }
                ]
            });

        _memberServiceMock
            .Setup(s => s.GetByIdsAsync(It.IsAny<string[]>(), MemberResponseGroup.WithRoles.ToString(), It.IsAny<string[]>()))
            .ReturnsAsync(
            [
                new Organization
                {
                    Id = "org-1",
                    Roles = [new OrganizationRole { RoleId = "role1", RoleName = "Admin" }]
                }
            ]);

        var doc = new IndexDocument("c1");

        //Act
        await GetBuilder().IndexRolesAsync(doc, contact);

        //Assert — "role1" indexed once from membership, not again from org
        var roleIds = doc.Fields.FirstOrDefault(f => f.Name == "RoleId")?.Values.OfType<string>().ToList() ?? [];

        Assert.Equal(1, roleIds.Count(id => id == "role1"));
    }

    [Fact]
    public async Task IndexOrganizationMembershipRolesAsync_MultipleUsers_SingleGetByIdsCallWithDistinctOrgs()
    {
        // Two users both in same org → GetByIdsAsync called once, not twice.
        //Arrange
        var contact = new Contact
        {
            Id = "c1",
            SecurityAccounts =
            [
                new ApplicationUser { Id = "user-1", Roles = [] },
                new ApplicationUser { Id = "user-2", Roles = [] },
            ]
        };

        _membershipServiceMock
            .Setup(s => s.SearchAsync(
                It.Is<OrganizationMembershipSearchCriteria>(c => c.UserId == "user-1"),
                It.IsAny<bool>()))
            .ReturnsAsync(new OrganizationMembershipSearchResult
            {
                Results = [new OrganizationMembership { OrganizationId = "org-1", Roles = [] }]
            });

        _membershipServiceMock
            .Setup(s => s.SearchAsync(
                It.Is<OrganizationMembershipSearchCriteria>(c => c.UserId == "user-2"),
                It.IsAny<bool>()))
            .ReturnsAsync(new OrganizationMembershipSearchResult
            {
                Results = [new OrganizationMembership { OrganizationId = "org-1", Roles = [] }]
            });

        _memberServiceMock
            .Setup(s => s.GetByIdsAsync(It.IsAny<string[]>(), MemberResponseGroup.WithRoles.ToString(), It.IsAny<string[]>()))
            .ReturnsAsync([]);

        var doc = new IndexDocument("c1");

        //Act
        await GetBuilder().IndexRolesAsync(doc, contact);

        //Assert — SearchAsync called per user, GetByIdsAsync once with deduplicated org IDs
        _membershipServiceMock.Verify(
            s => s.SearchAsync(It.IsAny<OrganizationMembershipSearchCriteria>(), It.IsAny<bool>()),
            Times.Exactly(2));

        _memberServiceMock.Verify(
            s => s.GetByIdsAsync(
                It.Is<string[]>(ids => ids.Length == 1 && ids[0] == "org-1"),
                MemberResponseGroup.WithRoles.ToString(),
                It.IsAny<string[]>()),
            Times.Once);
    }

    private TestableMemberDocumentBuilder GetBuilder() =>
        new(_memberServiceMock.Object, _membershipServiceMock.Object);

    private sealed class TestableMemberDocumentBuilder(
        IMemberService memberService,
        IOrganizationMembershipService organizationMembershipService)
        : MemberDocumentBuilder(memberService, new Mock<IDynamicPropertySearchService>().Object, organizationMembershipService)
    {
        public Task IndexRolesAsync(IndexDocument document, Contact contact) =>
            IndexOrganizationMembershipRolesAsync(document, contact);
    }
}
