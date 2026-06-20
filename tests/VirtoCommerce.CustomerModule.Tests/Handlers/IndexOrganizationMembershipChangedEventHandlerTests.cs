using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Data.Handlers;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.SearchModule.Core.BackgroundJobs;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests.Handlers
{
    [Trait("Category", "CI")]
    public class IndexOrganizationMembershipChangedEventHandlerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IIndexingJobService> _indexingJobServiceMock = new();
        private readonly IndexOrganizationMembershipChangedEventHandler _handler;

        public IndexOrganizationMembershipChangedEventHandlerTests()
        {
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                new Mock<IUserStore<ApplicationUser>>().Object, null, null, null, null, null, null, null, null);

            _handler = new IndexOrganizationMembershipChangedEventHandler(
                () => _userManagerMock.Object,
                _indexingJobServiceMock.Object,
                []);
        }

        [Fact]
        public async Task Handle_WhenMembershipAdded_EnqueuesReindexForMember()
        {
            var user = new ApplicationUser { Id = "user-1", MemberId = "member-1" };
            _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);

            var membership = new OrganizationMembership { UserId = "user-1" };
            var message = BuildEvent(membership, EntryState.Added);

            await _handler.Handle(message);

            _indexingJobServiceMock.Verify(s =>
                s.EnqueueIndexAndDeleteDocuments(
                    It.Is<IndexEntry[]>(entries =>
                        entries.Length == 1 &&
                        entries[0].Id == "member-1" &&
                        entries[0].EntryState == EntryState.Modified &&
                        entries[0].Type == KnownDocumentTypes.Member),
                    It.IsAny<string>(),
                    It.IsAny<IList<IIndexDocumentBuilder>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenMembershipModified_EnqueuesReindexForMember()
        {
            var user = new ApplicationUser { Id = "user-1", MemberId = "member-1" };
            _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);

            var membership = new OrganizationMembership { UserId = "user-1" };
            var message = BuildEvent(membership, EntryState.Modified);

            await _handler.Handle(message);

            _indexingJobServiceMock.Verify(s =>
                s.EnqueueIndexAndDeleteDocuments(
                    It.Is<IndexEntry[]>(entries => entries.Length == 1 && entries[0].Id == "member-1"),
                    It.IsAny<string>(),
                    It.IsAny<IList<IIndexDocumentBuilder>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenDeletedEntry_UsesOldEntryUserId()
        {
            var user = new ApplicationUser { Id = "user-1", MemberId = "member-1" };
            _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);

            var oldMembership = new OrganizationMembership { UserId = "user-1" };
            var message = new OrganizationMembershipChangedEvent(
                new List<GenericChangedEntry<OrganizationMembership>>
                {
                    new(null, oldMembership, EntryState.Deleted),
                });

            await _handler.Handle(message);

            _indexingJobServiceMock.Verify(s =>
                s.EnqueueIndexAndDeleteDocuments(
                    It.Is<IndexEntry[]>(entries => entries.Length == 1 && entries[0].Id == "member-1"),
                    It.IsAny<string>(),
                    It.IsAny<IList<IIndexDocumentBuilder>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_WhenUserNotFound_DoesNotEnqueueReindex()
        {
            _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

            var membership = new OrganizationMembership { UserId = "user-1" };
            var message = BuildEvent(membership, EntryState.Modified);

            await _handler.Handle(message);

            _indexingJobServiceMock.Verify(s =>
                s.EnqueueIndexAndDeleteDocuments(
                    It.IsAny<IndexEntry[]>(),
                    It.IsAny<string>(),
                    It.IsAny<IList<IIndexDocumentBuilder>>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenUserHasNoMemberId_DoesNotEnqueueReindex()
        {
            var user = new ApplicationUser { Id = "user-1", MemberId = null };
            _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);

            var membership = new OrganizationMembership { UserId = "user-1" };
            var message = BuildEvent(membership, EntryState.Modified);

            await _handler.Handle(message);

            _indexingJobServiceMock.Verify(s =>
                s.EnqueueIndexAndDeleteDocuments(
                    It.IsAny<IndexEntry[]>(),
                    It.IsAny<string>(),
                    It.IsAny<IList<IIndexDocumentBuilder>>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_WhenMultipleEntriesForSameUser_DeduplicatesUserIds()
        {
            var user = new ApplicationUser { Id = "user-1", MemberId = "member-1" };
            _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);

            var membership = new OrganizationMembership { UserId = "user-1" };
            var message = new OrganizationMembershipChangedEvent(
                new List<GenericChangedEntry<OrganizationMembership>>
                {
                    new(membership, EntryState.Added),
                    new(membership, EntryState.Modified),
                });

            await _handler.Handle(message);

            _userManagerMock.Verify(m => m.FindByIdAsync("user-1"), Times.Once);
            _indexingJobServiceMock.Verify(s =>
                s.EnqueueIndexAndDeleteDocuments(
                    It.Is<IndexEntry[]>(entries => entries.Length == 1 && entries[0].Id == "member-1"),
                    It.IsAny<string>(),
                    It.IsAny<IList<IIndexDocumentBuilder>>()),
                Times.Once);
        }

        private static OrganizationMembershipChangedEvent BuildEvent(OrganizationMembership membership, EntryState state)
        {
            return new OrganizationMembershipChangedEvent(
                new List<GenericChangedEntry<OrganizationMembership>>
                {
                    new(membership, state),
                });
        }
    }
}
