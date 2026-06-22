using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Data.Handlers;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests.Handlers
{
    [Trait("Category", "CI")]
    public class RevokeTokenOrganizationMembershipChangedEventHandlerTests
    {
        private readonly Mock<IUserSessionsService> _sessionServiceMock = new();
        private readonly RevokeTokenOrganizationMembershipChangedEventHandler _handler;

        public RevokeTokenOrganizationMembershipChangedEventHandlerTests()
        {
            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(s => s.ServiceProvider).Returns(Mock.Of<IServiceProvider>());

            _handler = new RevokeTokenOrganizationMembershipChangedEventHandler(
                () => (_sessionServiceMock.Object, scopeMock.Object));
        }

        [Fact]
        public async Task Handle_WhenMembershipIsCurrentlyLocked_RevokesUserTokens()
        {
            var membership = new OrganizationMembership
            {
                UserId = "user-1",
                IsLocked = true,
            };

            var message = BuildEvent(membership, EntryState.Modified);

            await _handler.Handle(message);

            _sessionServiceMock.Verify(s => s.TerminateAllUserSessions("user-1"), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenMembershipIsNotLocked_DoesNotRevokeTokens()
        {
            var membership = new OrganizationMembership
            {
                UserId = "user-1",
                IsLocked = false,
            };

            var message = BuildEvent(membership, EntryState.Modified);

            await _handler.Handle(message);

            _sessionServiceMock.Verify(s => s.TerminateAllUserSessions(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenLockoutEndIsInPast_DoesNotRevokeTokens()
        {
            var membership = new OrganizationMembership
            {
                UserId = "user-1",
                IsLocked = true,
                LockoutEnd = DateTime.UtcNow.AddDays(-1),
            };

            var message = BuildEvent(membership, EntryState.Modified);

            await _handler.Handle(message);

            _sessionServiceMock.Verify(s => s.TerminateAllUserSessions(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenEntryStateIsDeleted_DoesNotRevokeTokens()
        {
            var membership = new OrganizationMembership
            {
                UserId = "user-1",
                IsLocked = true,
            };

            var message = BuildEvent(membership, EntryState.Deleted);

            await _handler.Handle(message);

            _sessionServiceMock.Verify(s => s.TerminateAllUserSessions(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WhenEntryStateIsAdded_AndLocked_RevokesTokens()
        {
            var membership = new OrganizationMembership
            {
                UserId = "user-1",
                IsLocked = true,
            };

            var message = BuildEvent(membership, EntryState.Added);

            await _handler.Handle(message);

            _sessionServiceMock.Verify(s => s.TerminateAllUserSessions("user-1"), Times.Once);
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
