using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.Handlers;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.Events;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests.Handlers
{
    [Trait("Category", "CI")]
    public class DeleteOrganizationMembershipUserChangedEventHandlerTests
    {
        private const string UserId = "user1";

        private readonly Mock<IOrganizationMembershipService> _membershipServiceMock = new();
        private readonly DeleteOrganizationMembershipUserChangedEventHandler _handler;

        public DeleteOrganizationMembershipUserChangedEventHandlerTests()
        {
            _handler = new DeleteOrganizationMembershipUserChangedEventHandler(_membershipServiceMock.Object);
        }

        [Fact]
        public async Task Handle_UserDeleted_DeletesMemberships()
        {
            //Arrange
            _membershipServiceMock
                .Setup(s => s.SearchAsync(It.Is<OrganizationMembershipSearchCriteria>(c => c.UserId == UserId)))
                .ReturnsAsync(new OrganizationMembershipSearchResult
                {
                    Results = [new OrganizationMembership { Id = "m1" }, new OrganizationMembership { Id = "m2" }]
                });

            //Act
            await _handler.Handle(BuildEvent(UserId, EntryState.Deleted));

            //Assert
            _membershipServiceMock.Verify(
                s => s.DeleteAsync(It.Is<IList<string>>(ids => ids.Count == 2)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_UserDeleted_NoMemberships_DoesNotCallDelete()
        {
            //Arrange
            _membershipServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<OrganizationMembershipSearchCriteria>()))
                .ReturnsAsync(new OrganizationMembershipSearchResult { Results = [] });

            //Act
            await _handler.Handle(BuildEvent(UserId, EntryState.Deleted));

            //Assert
            _membershipServiceMock.Verify(s => s.DeleteAsync(It.IsAny<IList<string>>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UserModified_DoesNotDeleteMemberships()
        {
            //Act
            await _handler.Handle(BuildEvent(UserId, EntryState.Modified));

            //Assert
            _membershipServiceMock.Verify(s => s.SearchAsync(It.IsAny<OrganizationMembershipSearchCriteria>()), Times.Never);
            _membershipServiceMock.Verify(s => s.DeleteAsync(It.IsAny<IList<string>>()), Times.Never);
        }

        [Fact]
        public async Task Handle_MultipleDeletedUsers_DeletesMembershipsForEach()
        {
            //Arrange
            var userId2 = "user2";
            _membershipServiceMock
                .Setup(s => s.SearchAsync(It.Is<OrganizationMembershipSearchCriteria>(c => c.UserId == UserId)))
                .ReturnsAsync(new OrganizationMembershipSearchResult { Results = [new OrganizationMembership { Id = "m1" }] });
            _membershipServiceMock
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

        private static UserChangedEvent BuildEvent(string userId, EntryState state)
        {
            var user = new ApplicationUser { Id = userId };
            return new UserChangedEvent(
                new List<GenericChangedEntry<ApplicationUser>> { new(user, user, state) });
        }
    }
}
