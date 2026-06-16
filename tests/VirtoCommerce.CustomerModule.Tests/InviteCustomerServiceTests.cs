using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.NotificationsModule.Core.Services;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.StoreModule.Core.Services;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests;

public class InviteCustomerServiceTests
{
    private readonly Mock<IMemberService> _memberServiceMock = new();
    private readonly Mock<IStoreService> _storeServiceMock = new();
    private readonly Mock<INotificationSearchService> _notificationSearchServiceMock = new();
    private readonly Mock<INotificationSender> _notificationSenderMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<Role>> _roleManagerMock;
    private readonly Mock<IOrganizationMembershipService> _membershipServiceMock = new();
    private readonly Mock<ILogger<InviteCustomerService>> _loggerMock = new();

    public InviteCustomerServiceTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object, null, null, null, null, null, null, null, null);
        _roleManagerMock = new Mock<RoleManager<Role>>(
            new Mock<IRoleStore<Role>>().Object, null, null, null, null);
    }

    [Fact]
    public void CreateUser_WithOrganizationId_DoesNotAssignRolesGlobally()
    {
        // Arrange
        var service = BuildTestableService();
        var request = new InviteCustomerRequest { OrganizationId = "org-1", StoreId = "store-1" };
        var roles = new List<Role> { new() { Id = "role-1", Name = "TestRole" } };

        // Act
        var user = service.InvokeCreateUser(request, new Contact { Id = "contact-1" }, "test@example.com", roles);

        // Assert — with an org context, roles go into OrganizationMembership, not the global account
        Assert.True(user.Roles == null || !user.Roles.Any(r => r.Id == "role-1"),
            "Roles must not be assigned globally when OrganizationId is set");
    }

    [Fact]
    public void CreateUser_WithoutOrganizationId_AssignsRolesGlobally()
    {
        // Arrange
        var service = BuildTestableService();
        var request = new InviteCustomerRequest { OrganizationId = null, StoreId = "store-1" };
        var roles = new List<Role> { new() { Id = "role-1", Name = "TestRole" } };

        // Act
        var user = service.InvokeCreateUser(request, new Contact { Id = "contact-1" }, "test@example.com", roles);

        // Assert — without an org context, global role assignment is still the correct path
        Assert.NotNull(user.Roles);
        Assert.Contains(user.Roles, r => r.Id == "role-1");
    }

    [Fact]
    public async Task CreateOrganizationMembershipAsync_SavesMembershipWithCorrectData()
    {
        // Arrange
        var service = BuildTestableService();
        var user = new ApplicationUser { Id = "user-1" };
        var roles = new List<Role>
        {
            new() { Id = "role-1", Name = "Role One" },
            new() { Id = "role-2", Name = "Role Two" },
        };

        // Act
        await service.InvokeCreateOrganizationMembershipAsync(user, "org-1", roles);

        // Assert
        _membershipServiceMock.Verify(
            x => x.SaveChangesAsync(It.Is<IList<OrganizationMembership>>(list =>
                list.Count == 1 &&
                list[0].UserId == "user-1" &&
                list[0].OrganizationId == "org-1" &&
                list[0].Roles.Count == 2 &&
                list[0].Roles.Any(r => r.RoleId == "role-1" && r.RoleName == "Role One") &&
                list[0].Roles.Any(r => r.RoleId == "role-2" && r.RoleName == "Role Two"))),
            Times.Once);
    }

    private TestableInviteCustomerService BuildTestableService() =>
        new(
            _memberServiceMock.Object,
            _storeServiceMock.Object,
            _notificationSearchServiceMock.Object,
            _notificationSenderMock.Object,
            () => _userManagerMock.Object,
            () => _roleManagerMock.Object,
            _membershipServiceMock.Object,
            _loggerMock.Object);

    private sealed class TestableInviteCustomerService(
        IMemberService memberService,
        IStoreService storeService,
        INotificationSearchService notificationSearchService,
        INotificationSender notificationSender,
        Func<UserManager<ApplicationUser>> userManagerFactory,
        Func<RoleManager<Role>> roleManagerFactory,
        IOrganizationMembershipService organizationMembershipService,
        ILogger<InviteCustomerService> logger)
        : InviteCustomerService(memberService, storeService, notificationSearchService, notificationSender,
            userManagerFactory, roleManagerFactory, organizationMembershipService, logger)
    {
        public ApplicationUser InvokeCreateUser(
            InviteCustomerRequest request, Contact contact, string email, List<Role> roles)
            => CreateUser(request, contact, email, roles);

        public Task InvokeCreateOrganizationMembershipAsync(
            ApplicationUser user, string organizationId, List<Role> roles)
            => CreateOrganizationMembershipAsync(user, organizationId, roles);
    }
}
