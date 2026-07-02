using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.Handlers;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.SearchModule.Core.BackgroundJobs;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests.Handlers;

[Trait("Category", "CI")]
public class IndexOrganizationMembersChangedEventHandlerTests
{
    private readonly Mock<IMemberSearchService> _memberSearchServiceMock = new();
    private readonly Mock<IIndexingJobService> _indexingJobServiceMock = new();
    private readonly IndexOrganizationMembersChangedEventHandler _handler;

    public IndexOrganizationMembersChangedEventHandlerTests()
    {
        _handler = new IndexOrganizationMembersChangedEventHandler(
            _memberSearchServiceMock.Object,
            _indexingJobServiceMock.Object,
            []);
    }

    [Fact]
    public async Task Handle_OrganizationRolesModified_EnqueuesReindexForMembers()
    {
        //Arrange
        _memberSearchServiceMock
            .Setup(s => s.SearchAllAsync(It.Is<MembersSearchCriteria>(c => c.MemberId == "org-1")))
            .ReturnsAsync(
                [
                    new Contact { Id = "contact-1" },
                    new Contact { Id = "contact-2" }
                ]);

        var message = BuildRolesChangedEvent("org-1");

        //Act
        await _handler.Handle(message);

        //Assert
        _indexingJobServiceMock.Verify(s =>
            s.EnqueueIndexAndDeleteDocuments(
                It.Is<IndexEntry[]>(entries =>
                    entries.Length == 2 &&
                    entries.All(e => e.EntryState == EntryState.Modified && e.Type == KnownDocumentTypes.Member) &&
                    entries.Any(e => e.Id == "contact-1") &&
                    entries.Any(e => e.Id == "contact-2")),
                It.IsAny<string>(),
                It.IsAny<IList<IIndexDocumentBuilder>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_OrganizationModifiedWithoutRoleChanges_DoesNotEnqueue()
    {
        // A rename/description edit must not cascade to members — only role changes do.
        //Arrange
        var message = new MemberChangedEvent(
        [
            ModifiedOrgEntry("org-1", oldRoleIds: ["r1"], newRoleIds: ["r1"]),
        ]);

        //Act
        await _handler.Handle(message);

        //Assert
        _indexingJobServiceMock.Verify(s =>
            s.EnqueueIndexAndDeleteDocuments(
                It.IsAny<IndexEntry[]>(),
                It.IsAny<string>(),
                It.IsAny<IList<IIndexDocumentBuilder>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_OrganizationModifiedWithNullRoles_DoesNotEnqueue()
    {
        // Null Roles on the saved model means roles were not managed by the caller
        // (the persistence layer leaves them intact), so no role change happened.
        //Arrange
        var message = new MemberChangedEvent(
        [
            new(new Organization { Id = "org-1" }, BuildOrg("org-1", "r1"), EntryState.Modified),
        ]);

        //Act
        await _handler.Handle(message);

        //Assert
        _indexingJobServiceMock.Verify(s =>
            s.EnqueueIndexAndDeleteDocuments(
                It.IsAny<IndexEntry[]>(),
                It.IsAny<string>(),
                It.IsAny<IList<IIndexDocumentBuilder>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_OrganizationRoleRenamed_EnqueuesReindexForMembers()
    {
        // Role names are indexed in member documents, so a rename (same RoleId) must reindex members.
        //Arrange
        _memberSearchServiceMock
            .Setup(s => s.SearchAllAsync(It.Is<MembersSearchCriteria>(c => c.MemberId == "org-1")))
            .ReturnsAsync([new Contact { Id = "contact-1" }]);

        var oldOrg = new Organization
        {
            Id = "org-1",
            Roles =
            [
                new OrganizationRole { RoleId = "r1", RoleName = "Buyer" }
            ]
        };

        var newOrg = new Organization
        {
            Id = "org-1",
            Roles =
            [
                new OrganizationRole { RoleId = "r1", RoleName = "Purchaser" }
            ]
        };

        var message = new MemberChangedEvent(
        [
            new(newOrg, oldOrg, EntryState.Modified),
        ]);

        //Act
        await _handler.Handle(message);

        //Assert
        _indexingJobServiceMock.Verify(s =>
            s.EnqueueIndexAndDeleteDocuments(
                It.Is<IndexEntry[]>(entries => entries.Length == 1 && entries[0].Id == "contact-1"),
                It.IsAny<string>(),
                It.IsAny<IList<IIndexDocumentBuilder>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_OrganizationRoleRemoved_EnqueuesReindexForMembers()
    {
        // A role removed from the org (no longer assignable) is also a role-set change, not just additions.
        //Arrange
        _memberSearchServiceMock
            .Setup(s => s.SearchAllAsync(It.Is<MembersSearchCriteria>(c => c.MemberId == "org-1")))
            .ReturnsAsync([new Contact { Id = "contact-1" }]);

        var oldOrg = new Organization { Id = "org-1", Roles = [new OrganizationRole { RoleId = "r1", RoleName = "Buyer" }] };
        var newOrg = new Organization { Id = "org-1", Roles = [] };
        var message = new MemberChangedEvent(
        [
            new(newOrg, oldOrg, EntryState.Modified),
        ]);

        //Act
        await _handler.Handle(message);

        //Assert
        _indexingJobServiceMock.Verify(s =>
            s.EnqueueIndexAndDeleteDocuments(
                It.Is<IndexEntry[]>(entries => entries.Length == 1 && entries[0].Id == "contact-1"),
                It.IsAny<string>(),
                It.IsAny<IList<IIndexDocumentBuilder>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_OrganizationAdded_DoesNotEnqueue()
    {
        // Only Modified orgs trigger member reindex (org roles changed → propagate to members).
        //Arrange
        var org = new Organization { Id = "org-1" };
        var message = new MemberChangedEvent(
        [
            new(org, EntryState.Added),
        ]);

        //Act
        await _handler.Handle(message);

        //Assert
        _indexingJobServiceMock.Verify(s =>
            s.EnqueueIndexAndDeleteDocuments(
                It.IsAny<IndexEntry[]>(),
                It.IsAny<string>(),
                It.IsAny<IList<IIndexDocumentBuilder>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ContactModified_DoesNotEnqueue()
    {
        // Non-organization members do not trigger member reindex.
        //Arrange
        var contact = new Contact { Id = "c-1" };
        var message = new MemberChangedEvent(
            [
                new(contact, EntryState.Modified),
            ]);

        //Act
        await _handler.Handle(message);

        //Assert
        _indexingJobServiceMock.Verify(s =>
            s.EnqueueIndexAndDeleteDocuments(
                It.IsAny<IndexEntry[]>(),
                It.IsAny<string>(),
                It.IsAny<IList<IIndexDocumentBuilder>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_OrganizationWithNoMembers_DoesNotEnqueue()
    {
        //Arrange
        _memberSearchServiceMock
            .Setup(s => s.SearchAllAsync(It.IsAny<MembersSearchCriteria>()))
            .ReturnsAsync([]);

        var message = BuildRolesChangedEvent("org-1");

        //Act
        await _handler.Handle(message);

        //Assert
        _indexingJobServiceMock.Verify(s =>
            s.EnqueueIndexAndDeleteDocuments(
                It.IsAny<IndexEntry[]>(),
                It.IsAny<string>(),
                It.IsAny<IList<IIndexDocumentBuilder>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_MultipleOrganizations_DeduplicatesMembers()
    {
        // A contact belonging to two orgs should be reindexed once, not twice.
        //Arrange
        var sharedContact = new Contact { Id = "contact-shared" };
        _memberSearchServiceMock
            .Setup(s => s.SearchAllAsync(It.Is<MembersSearchCriteria>(c => c.MemberId == "org-1")))
            .ReturnsAsync([sharedContact]);
        _memberSearchServiceMock
            .Setup(s => s.SearchAllAsync(It.Is<MembersSearchCriteria>(c => c.MemberId == "org-2")))
            .ReturnsAsync([sharedContact]);

        var message = new MemberChangedEvent(
        [
            ModifiedOrgEntry("org-1", oldRoleIds: [], newRoleIds: ["r1"]),
            ModifiedOrgEntry("org-2", oldRoleIds: [], newRoleIds: ["r1"]),
        ]);

        //Act
        await _handler.Handle(message);

        //Assert
        _indexingJobServiceMock.Verify(s =>
            s.EnqueueIndexAndDeleteDocuments(
                It.Is<IndexEntry[]>(entries => entries.Length == 1 && entries[0].Id == "contact-shared"),
                It.IsAny<string>(),
                It.IsAny<IList<IIndexDocumentBuilder>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyMessage_DoesNotEnqueue()
    {
        //Arrange
        var message = new MemberChangedEvent([]);

        //Act
        await _handler.Handle(message);

        //Assert
        _indexingJobServiceMock.Verify(s =>
            s.EnqueueIndexAndDeleteDocuments(
                It.IsAny<IndexEntry[]>(),
                It.IsAny<string>(),
                It.IsAny<IList<IIndexDocumentBuilder>>()),
            Times.Never);
    }

    private static MemberChangedEvent BuildRolesChangedEvent(string orgId) =>
        new(
        [
            ModifiedOrgEntry(orgId, oldRoleIds: [], newRoleIds: ["r1"]),
        ]);

    private static GenericChangedEntry<Member> ModifiedOrgEntry(string orgId, string[] oldRoleIds, string[] newRoleIds) =>
        new(BuildOrg(orgId, newRoleIds), BuildOrg(orgId, oldRoleIds), EntryState.Modified);

    private static Organization BuildOrg(string id, params string[] roleIds) =>
        new()
        {
            Id = id,
            Roles = roleIds.Select(rid => new OrganizationRole { RoleId = rid }).ToList(),
        };
}
