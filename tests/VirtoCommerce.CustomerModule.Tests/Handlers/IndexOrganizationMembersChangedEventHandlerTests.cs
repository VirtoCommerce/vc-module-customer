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
    public async Task Handle_OrganizationModified_EnqueuesReindexForMembers()
    {
        //Arrange
        var org = new Organization { Id = "org-1" };
        _memberSearchServiceMock
            .Setup(s => s.SearchMembersAsync(It.Is<MembersSearchCriteria>(c => c.MemberId == "org-1")))
            .ReturnsAsync(new MemberSearchResult
            {
                Results = [new Contact { Id = "contact-1" }, new Contact { Id = "contact-2" }]
            });

        var message = BuildEvent(org, EntryState.Modified);

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
    public async Task Handle_OrganizationAdded_DoesNotEnqueue()
    {
        // Only Modified orgs trigger member reindex (org roles changed → propagate to members).
        //Arrange
        var org = new Organization { Id = "org-1" };
        var message = BuildEvent(org, EntryState.Added);

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
        var org = new Organization { Id = "org-1" };
        _memberSearchServiceMock
            .Setup(s => s.SearchMembersAsync(It.IsAny<MembersSearchCriteria>()))
            .ReturnsAsync(new MemberSearchResult { Results = [] });

        var message = BuildEvent(org, EntryState.Modified);

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
            .Setup(s => s.SearchMembersAsync(It.Is<MembersSearchCriteria>(c => c.MemberId == "org-1")))
            .ReturnsAsync(new MemberSearchResult { Results = [sharedContact] });
        _memberSearchServiceMock
            .Setup(s => s.SearchMembersAsync(It.Is<MembersSearchCriteria>(c => c.MemberId == "org-2")))
            .ReturnsAsync(new MemberSearchResult { Results = [sharedContact] });

        var message = new MemberChangedEvent(
            [
                new(new Organization { Id = "org-1" }, EntryState.Modified),
                new(new Organization { Id = "org-2" }, EntryState.Modified),
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

    private static MemberChangedEvent BuildEvent(Organization org, EntryState state) =>
        new(
        [
            new(org, state),
        ]);
}
