using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.Handlers;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.Events;
using VirtoCommerce.Platform.Core.Security.Search;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests.Handlers
{
    [Trait("Category", "CI")]
    public class DeleteOrganizationMembershipUserChangedEventHandlerTests
    {
        private const string UserId = "user1";
        private const string MemberId = "contact1";
        private const string OrgId = "org1";

        private readonly Mock<IOrganizationMembershipSearchService> _membershipSearchServiceMock = new();
        private readonly Mock<IOrganizationMembershipService> _membershipServiceMock = new();
        private readonly Mock<IUserSearchService> _userSearchServiceMock = new();
        private readonly DeleteOrganizationMembershipUserChangedEventHandler _handler;

        public DeleteOrganizationMembershipUserChangedEventHandlerTests()
        {
            _handler = new DeleteOrganizationMembershipUserChangedEventHandler(
                _membershipSearchServiceMock.Object,
                _membershipServiceMock.Object,
                _userSearchServiceMock.Object);
        }

        [Fact]
        public async Task Handle_UserDeleted_DeletesMemberships()
        {
            //Arrange
            _membershipSearchServiceMock
                .Setup(s => s.SearchAsync(It.Is<OrganizationMembershipSearchCriteria>(c => c.UserId == UserId)))
                .ReturnsAsync(new OrganizationMembershipSearchResult
                {
                    Results = [new OrganizationMembership { Id = "m1" }, new OrganizationMembership { Id = "m2" }]
                });

            //Act
            await _handler.Handle(BuildUserEvent(UserId, EntryState.Deleted));

            //Assert
            _membershipServiceMock.Verify(
                s => s.DeleteAsync(It.Is<IList<string>>(ids => ids.Count == 2)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_UserDeleted_NoMemberships_DoesNotCallDelete()
        {
            //Arrange
            _membershipSearchServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<OrganizationMembershipSearchCriteria>()))
                .ReturnsAsync(new OrganizationMembershipSearchResult { Results = [] });

            //Act
            await _handler.Handle(BuildUserEvent(UserId, EntryState.Deleted));

            //Assert
            _membershipServiceMock.Verify(s => s.DeleteAsync(It.IsAny<IList<string>>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UserModified_DoesNotDeleteMemberships()
        {
            //Act
            await _handler.Handle(BuildUserEvent(UserId, EntryState.Modified));

            //Assert
            _membershipSearchServiceMock.Verify(s => s.SearchAsync(It.IsAny<OrganizationMembershipSearchCriteria>()), Times.Never);
            _membershipServiceMock.Verify(s => s.DeleteAsync(It.IsAny<IList<string>>()), Times.Never);
        }

        [Fact]
        public async Task Handle_MultipleDeletedUsers_DeletesMembershipsForEach()
        {
            //Arrange
            var userId2 = "user2";
            _membershipSearchServiceMock
                .Setup(s => s.SearchAsync(It.Is<OrganizationMembershipSearchCriteria>(c => c.UserId == UserId)))
                .ReturnsAsync(new OrganizationMembershipSearchResult { Results = [new OrganizationMembership { Id = "m1" }] });
            _membershipSearchServiceMock
                .Setup(s => s.SearchAsync(It.Is<OrganizationMembershipSearchCriteria>(c => c.UserId == userId2)))
                .ReturnsAsync(new OrganizationMembershipSearchResult { Results = [new OrganizationMembership { Id = "m2" }] });

            var @event = new UserChangedEvent(new List<GenericChangedEntry<ApplicationUser>>
            {
                new(new ApplicationUser { Id = UserId }, new ApplicationUser { Id = UserId }, EntryState.Deleted),
                new(new ApplicationUser { Id = userId2 }, new ApplicationUser { Id = userId2 }, EntryState.Deleted),
            });

            //Act
            await _handler.Handle(@event);

            //Assert
            _membershipServiceMock.Verify(s => s.DeleteAsync(It.IsAny<IList<string>>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_ContactDeleted_DeletesMembershipsViaUserSearch()
        {
            //Arrange
            var contact = new Contact { Id = MemberId };
            _userSearchServiceMock
                .Setup(s => s.SearchUsersAsync(It.Is<UserSearchCriteria>(c => c.MemberIds.Contains(MemberId))))
                .ReturnsAsync(new UserSearchResult { Results = [new ApplicationUser { Id = UserId }] });
            _membershipSearchServiceMock
                .Setup(s => s.SearchAsync(It.Is<OrganizationMembershipSearchCriteria>(c => c.UserId == UserId)))
                .ReturnsAsync(new OrganizationMembershipSearchResult
                {
                    Results = [new OrganizationMembership { Id = "m1" }]
                });

            //Act
            await _handler.Handle(BuildMemberEvent(contact, contact, EntryState.Deleted));

            //Assert
            _membershipServiceMock.Verify(
                s => s.DeleteAsync(It.Is<IList<string>>(ids => ids.Count == 1)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ContactDeleted_NoUsers_DoesNotCallDelete()
        {
            //Arrange
            var contact = new Contact { Id = MemberId };
            _userSearchServiceMock
                .Setup(s => s.SearchUsersAsync(It.IsAny<UserSearchCriteria>()))
                .ReturnsAsync(new UserSearchResult { Results = [] });

            //Act
            await _handler.Handle(BuildMemberEvent(contact, contact, EntryState.Deleted));

            //Assert
            _membershipSearchServiceMock.Verify(s => s.SearchAsync(It.IsAny<OrganizationMembershipSearchCriteria>()), Times.Never);
            _membershipServiceMock.Verify(s => s.DeleteAsync(It.IsAny<IList<string>>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ContactRemovedFromOrganization_DeletesMembershipForThatOrg()
        {
            //Arrange
            var oldContact = new Contact { Id = MemberId, Organizations = [OrgId, "org2"] };
            var newContact = new Contact { Id = MemberId, Organizations = ["org2"] };
            _userSearchServiceMock
                .Setup(s => s.SearchUsersAsync(It.Is<UserSearchCriteria>(c => c.MemberIds.Contains(MemberId))))
                .ReturnsAsync(new UserSearchResult { Results = [new ApplicationUser { Id = UserId }] });
            _membershipSearchServiceMock
                .Setup(s => s.SearchAsync(It.Is<OrganizationMembershipSearchCriteria>(c => c.UserId == UserId)))
                .ReturnsAsync(new OrganizationMembershipSearchResult
                {
                    Results =
                    [
                        new OrganizationMembership { Id = "m1", OrganizationId = OrgId },
                        new OrganizationMembership { Id = "m2", OrganizationId = "org2" }
                    ]
                });

            //Act
            await _handler.Handle(BuildMemberEvent(oldContact, newContact, EntryState.Modified));

            //Assert
            _membershipServiceMock.Verify(
                s => s.DeleteAsync(It.Is<IList<string>>(ids => ids.Count == 1 && ids[0] == "m1")),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ContactRemovedFromOrganization_NoMembership_DoesNotCallDelete()
        {
            //Arrange
            var oldContact = new Contact { Id = MemberId, Organizations = [OrgId] };
            var newContact = new Contact { Id = MemberId, Organizations = [] };
            _userSearchServiceMock
                .Setup(s => s.SearchUsersAsync(It.IsAny<UserSearchCriteria>()))
                .ReturnsAsync(new UserSearchResult { Results = [new ApplicationUser { Id = UserId }] });
            _membershipSearchServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<OrganizationMembershipSearchCriteria>()))
                .ReturnsAsync(new OrganizationMembershipSearchResult { Results = [] });

            //Act
            await _handler.Handle(BuildMemberEvent(oldContact, newContact, EntryState.Modified));

            //Assert
            _membershipServiceMock.Verify(s => s.DeleteAsync(It.IsAny<IList<string>>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ContactModified_OrganizationsUnchanged_DoesNotCallDelete()
        {
            //Arrange
            var contact = new Contact { Id = MemberId, Organizations = [OrgId] };

            //Act
            await _handler.Handle(BuildMemberEvent(contact, contact, EntryState.Modified));

            //Assert
            _userSearchServiceMock.Verify(s => s.SearchUsersAsync(It.IsAny<UserSearchCriteria>()), Times.Never);
            _membershipServiceMock.Verify(s => s.DeleteAsync(It.IsAny<IList<string>>()), Times.Never);
        }

        private static UserChangedEvent BuildUserEvent(string userId, EntryState state)
        {
            var user = new ApplicationUser { Id = userId };
            return new UserChangedEvent(
                new List<GenericChangedEntry<ApplicationUser>> { new(user, user, state) });
        }

        private static MemberChangedEvent BuildMemberEvent(Member oldMember, Member newMember, EntryState state)
        {
            return new MemberChangedEvent(
                new List<GenericChangedEntry<Member>> { new(newMember, oldMember, state) });
        }
    }
}
