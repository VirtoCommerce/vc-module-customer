using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using VirtoCommerce.CustomerModule.Core;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Notifications;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.Services;
using VirtoCommerce.NotificationsModule.Core.Model;
using VirtoCommerce.NotificationsModule.Core.Services;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.StoreModule.Core.Model;
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
    private readonly Mock<IOrganizationMembershipSearchService> _membershipSearchServiceMock = new();
    private readonly Mock<ICompanyMemberRoleService> _companyMemberRoleServiceMock = new();
    private readonly Mock<ILogger<InviteCustomerService>> _loggerMock = new();

    public InviteCustomerServiceTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object, null, null, null, null, null, null, null, null);
        _userManagerMock.Setup(x => x.GetLoginsAsync(It.IsAny<ApplicationUser>())).ReturnsAsync([]);
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

    [Fact]
    public async Task InviteCustomerAsyc_NewUserWithoutOrganization_UsesCustomerInviteNewUserNotification()
    {
        // Arrange
        var store = new Store { Id = "store-1", Url = "https://store.test", Email = "store@test.com" };
        _storeServiceMock.Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([store]);

        _notificationSearchServiceMock
            .Setup(x => x.SearchNotificationsAsync(It.Is<NotificationSearchCriteria>(
                c => c.NotificationType == typeof(CustomerInviteNewUserEmailNotification).Name)))
            .ReturnsAsync(new NotificationSearchResult
            {
                Results = [new CustomerInviteNewUserEmailNotification()],
                TotalCount = 1,
            });

        _userManagerMock.Setup(x => x.FindByEmailAsync("new@test.com")).ReturnsAsync((ApplicationUser)null);
        _userManagerMock.Setup(x => x.FindByNameAsync("new@test.com")).ReturnsAsync((ApplicationUser)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>())).ReturnsAsync("token123");

        var service = BuildTestableService();

        var request = new InviteCustomerRequest
        {
            StoreId = "store-1",
            OrganizationId = null,
            Emails = ["new@test.com"],
        };

        // Act
        var result = await service.InviteCustomerAsyc(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.Succeeded);
        _notificationSenderMock.Verify(
            x => x.ScheduleSendNotificationAsync(It.IsAny<CustomerInviteNewUserEmailNotification>()),
            Times.Once);
    }

    [Fact]
    public async Task InviteCustomerAsyc_ExistingUserIntoOrganization_UsesExistingUserNotification_NoToken()
    {
        // Arrange
        var store = new Store { Id = "store-1", Url = "https://store.test", Email = "store@test.com" };
        _storeServiceMock.Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([store]);

        _notificationSearchServiceMock
            .Setup(x => x.SearchNotificationsAsync(It.Is<NotificationSearchCriteria>(
                c => c.NotificationType == typeof(OrganizationInviteExistingUserEmailNotification).Name)))
            .ReturnsAsync(new NotificationSearchResult
            {
                Results = [new OrganizationInviteExistingUserEmailNotification()],
                TotalCount = 1,
            });
        _notificationSearchServiceMock
            .Setup(x => x.SearchNotificationsAsync(It.Is<NotificationSearchCriteria>(
                c => c.NotificationType == typeof(OrganizationInviteNewUserEmailNotification).Name)))
            .ReturnsAsync(new NotificationSearchResult
            {
                Results = [new OrganizationInviteNewUserEmailNotification()],
                TotalCount = 1,
            });

        var existingUser = new ApplicationUser { Id = "user-1", Email = "existing@test.com", MemberId = "contact-1" };
        _userManagerMock.Setup(x => x.FindByEmailAsync("existing@test.com")).ReturnsAsync(existingUser);

        _membershipSearchServiceMock
            .Setup(x => x.SearchAsync(
                It.Is<OrganizationMembershipSearchCriteria>(c => c.UserId == "user-1" && c.OrganizationId == "org-1"),
                It.IsAny<bool>()))
            .ReturnsAsync(new OrganizationMembershipSearchResult { Results = [] });

        _memberServiceMock.Setup(x => x.GetByIdAsync("contact-1", null, null))
            .ReturnsAsync(new Contact { Id = "contact-1", Organizations = [] });

        var service = BuildTestableService();

        var request = new InviteCustomerRequest
        {
            StoreId = "store-1",
            OrganizationId = "org-1",
            Emails = ["existing@test.com"],
        };

        // Act
        var result = await service.InviteCustomerAsyc(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.Succeeded);
        _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
        _membershipServiceMock.Verify(
            x => x.SaveChangesAsync(It.Is<IList<OrganizationMembership>>(list =>
                list.Count == 1 && list[0].UserId == "user-1" && list[0].OrganizationId == "org-1")),
            Times.Once);
        _notificationSenderMock.Verify(
            x => x.ScheduleSendNotificationAsync(It.Is<Notification>(n =>
                ((OrganizationInviteExistingUserEmailNotification)n).InviteUrl.EndsWith("/account/dashboard", StringComparison.Ordinal))),
            Times.Once);
    }

    [Fact]
    public async Task InviteCustomerAsyc_ExistingUserIntoOrganization_PopulatesOrganizationAndCustomerName()
    {
        // Arrange
        var store = new Store { Id = "store-1", Url = "https://store.test", Email = "store@test.com" };
        _storeServiceMock.Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([store]);

        _notificationSearchServiceMock
            .Setup(x => x.SearchNotificationsAsync(It.IsAny<NotificationSearchCriteria>()))
            .ReturnsAsync(new NotificationSearchResult
            {
                Results = [new OrganizationInviteExistingUserEmailNotification()],
                TotalCount = 1,
            });

        var existingUser = new ApplicationUser { Id = "user-1", Email = "existing@test.com", MemberId = "contact-1" };
        _userManagerMock.Setup(x => x.FindByEmailAsync("existing@test.com")).ReturnsAsync(existingUser);

        _membershipSearchServiceMock
            .Setup(x => x.SearchAsync(
                It.Is<OrganizationMembershipSearchCriteria>(c => c.UserId == "user-1" && c.OrganizationId == "org-1"),
                It.IsAny<bool>()))
            .ReturnsAsync(new OrganizationMembershipSearchResult { Results = [] });

        _memberServiceMock.Setup(x => x.GetByIdAsync("contact-1", null, null))
            .ReturnsAsync(new Contact { Id = "contact-1", FullName = "Jane Doe", Organizations = [] });
        _memberServiceMock.Setup(x => x.GetByIdAsync("org-1", null, nameof(Organization)))
            .ReturnsAsync(new Organization { Id = "org-1", Name = "Acme Corp" });

        var service = BuildTestableService();

        var request = new InviteCustomerRequest
        {
            StoreId = "store-1",
            OrganizationId = "org-1",
            Emails = ["existing@test.com"],
        };

        // Act
        await service.InviteCustomerAsyc(request, TestContext.Current.CancellationToken);

        // Assert
        _notificationSenderMock.Verify(
            x => x.ScheduleSendNotificationAsync(It.Is<Notification>(n =>
                ((OrganizationInviteExistingUserEmailNotification)n).OrganizationName == "Acme Corp" &&
                ((OrganizationInviteExistingUserEmailNotification)n).CustomerName == "Jane Doe")),
            Times.Once);
    }

    [Fact]
    public async Task InviteCustomerAsyc_ExistingUserAlreadyInAnotherOrganization_AddsSecondOrganizationToContact()
    {
        // Arrange
        var store = new Store { Id = "store-1", Url = "https://store.test", Email = "store@test.com" };
        _storeServiceMock.Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([store]);

        _notificationSearchServiceMock
            .Setup(x => x.SearchNotificationsAsync(It.IsAny<NotificationSearchCriteria>()))
            .ReturnsAsync(new NotificationSearchResult
            {
                Results = [new OrganizationInviteExistingUserEmailNotification()],
                TotalCount = 1,
            });

        var existingUser = new ApplicationUser { Id = "user-1", Email = "existing@test.com", MemberId = "contact-1" };
        _userManagerMock.Setup(x => x.FindByEmailAsync("existing@test.com")).ReturnsAsync(existingUser);

        _membershipSearchServiceMock
            .Setup(x => x.SearchAsync(
                It.Is<OrganizationMembershipSearchCriteria>(c => c.UserId == "user-1" && c.OrganizationId == "org-2"),
                It.IsAny<bool>()))
            .ReturnsAsync(new OrganizationMembershipSearchResult { Results = [] });

        _memberServiceMock.Setup(x => x.GetByIdAsync("contact-1", null, null))
            .ReturnsAsync(new Contact { Id = "contact-1", Organizations = ["org-1"] });

        var service = BuildTestableService();

        var request = new InviteCustomerRequest
        {
            StoreId = "store-1",
            OrganizationId = "org-2",
            Emails = ["existing@test.com"],
        };

        // Act
        var result = await service.InviteCustomerAsyc(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.Succeeded);
        _memberServiceMock.Verify(
            x => x.SaveChangesAsync(It.Is<Member[]>(members =>
                members.Length == 1 &&
                members[0].Id == "contact-1" &&
                ((Contact)members[0]).Organizations.SequenceEqual(new[] { "org-1", "org-2" }))),
            Times.Once);
    }

    [Fact]
    public async Task InviteCustomerAsyc_OrganizationInviteWithDisallowedRole_ReturnsRoleNotAllowedError()
    {
        // Arrange
        var store = new Store { Id = "store-1", Url = "https://store.test", Email = "store@test.com" };
        _storeServiceMock.Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([store]);

        _roleManagerMock.Setup(x => x.FindByIdAsync("not-allowed-role"))
            .ReturnsAsync(new Role { Id = "not-allowed-role", Name = "not-allowed-role" });

        _companyMemberRoleServiceMock.Setup(x => x.GetAllowedRoleIdsAsync("store-1"))
            .ReturnsAsync(["org-maintainer", "org-employee"]);

        _userManagerMock.Setup(x => x.FindByEmailAsync("new@test.com")).ReturnsAsync((ApplicationUser)null);
        _userManagerMock.Setup(x => x.FindByNameAsync("new@test.com")).ReturnsAsync((ApplicationUser)null);

        var service = BuildTestableService();

        var request = new InviteCustomerRequest
        {
            StoreId = "store-1",
            OrganizationId = "org-1",
            Emails = ["new@test.com"],
            RoleIds = ["not-allowed-role"],
        };

        // Act
        var result = await service.InviteCustomerAsyc(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "RoleNotAllowed" && e.Parameter == "not-allowed-role");

        _membershipServiceMock.Verify(x => x.SaveChangesAsync(It.IsAny<IList<OrganizationMembership>>()), Times.Never);
    }

    [Fact]
    public async Task InviteCustomerAsyc_OrganizationInviteWithAllowedRole_Succeeds()
    {
        // Arrange
        var store = new Store { Id = "store-1", Url = "https://store.test", Email = "store@test.com" };
        _storeServiceMock.Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([store]);

        _roleManagerMock.Setup(x => x.FindByIdAsync("org-maintainer"))
            .ReturnsAsync(new Role { Id = "org-maintainer", Name = "org-maintainer" });

        _companyMemberRoleServiceMock.Setup(x => x.GetAllowedRoleIdsAsync("store-1"))
            .ReturnsAsync(["org-maintainer", "org-employee"]);

        _notificationSearchServiceMock
            .Setup(x => x.SearchNotificationsAsync(It.IsAny<NotificationSearchCriteria>()))
            .ReturnsAsync(new NotificationSearchResult
            {
                Results = [new OrganizationInviteNewUserEmailNotification()],
                TotalCount = 1,
            });

        _userManagerMock.Setup(x => x.FindByEmailAsync("new@test.com")).ReturnsAsync((ApplicationUser)null);
        _userManagerMock.Setup(x => x.FindByNameAsync("new@test.com")).ReturnsAsync((ApplicationUser)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>())).ReturnsAsync("token123");

        var service = BuildTestableService();

        var request = new InviteCustomerRequest
        {
            StoreId = "store-1",
            OrganizationId = "org-1",
            Emails = ["new@test.com"],
            RoleIds = ["org-maintainer"],
        };

        // Act
        var result = await service.InviteCustomerAsyc(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.Succeeded);

        _membershipServiceMock.Verify(
            x => x.SaveChangesAsync(It.Is<IList<OrganizationMembership>>(list =>
                list.Count == 1 && list[0].Roles.Any(r => r.RoleId == "org-maintainer"))),
            Times.Once);
    }

    [Fact]
    public async Task ResendInviteAsync_ExistingUserWithPassword_UsesExistingUserNotification_NoToken()
    {
        // Arrange
        var membership = new OrganizationMembership
        {
            Id = "m1",
            UserId = "user-1",
            OrganizationId = "org-1",
            Status = ModuleConstants.MembershipStatuses.Invited,
        };
        _membershipServiceMock.Setup(x => x.GetAsync(It.IsAny<IList<string>>()))
            .ReturnsAsync([membership]);

        var user = new ApplicationUser { Id = "user-1", Email = "existing@test.com", StoreId = "store-1", PasswordHash = "somehash" };
        _userManagerMock.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);

        var store = new Store { Id = "store-1", Url = "https://store.test", Email = "store@test.com" };
        _storeServiceMock.Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([store]);

        _notificationSearchServiceMock
            .Setup(x => x.SearchNotificationsAsync(It.IsAny<NotificationSearchCriteria>()))
            .ReturnsAsync(new NotificationSearchResult
            {
                Results = [new OrganizationInviteExistingUserEmailNotification()],
                TotalCount = 1,
            });

        var service = BuildTestableService();

        // Act
        var result = await service.ResendInviteAsync(new ResendInviteRequest { MembershipId = "m1" }, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.Succeeded);
        _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
        _notificationSearchServiceMock.Verify(
            x => x.SearchNotificationsAsync(It.IsAny<NotificationSearchCriteria>()),
            Times.AtLeastOnce);
        _notificationSenderMock.Verify(
            x => x.ScheduleSendNotificationAsync(It.Is<Notification>(n =>
                ((OrganizationInviteExistingUserEmailNotification)n).InviteUrl.EndsWith("/account/dashboard", StringComparison.Ordinal))),
            Times.Once);
    }

    [Fact]
    public async Task ResendInviteAsync_ExistingUserWithCustomUrlSuffix_IgnoresCustomUrl()
    {
        // Arrange — a caller-supplied UrlSuffix must be ignored for existing users: the storefront always
        // sends the new-user registration path, which carries no token for this notification to use.
        var membership = new OrganizationMembership
        {
            Id = "m1",
            UserId = "user-1",
            OrganizationId = "org-1",
            Status = ModuleConstants.MembershipStatuses.Invited,
        };

        _membershipServiceMock.Setup(x => x.GetAsync(It.IsAny<IList<string>>()))
            .ReturnsAsync([membership]);

        var user = new ApplicationUser { Id = "user-1", Email = "existing@test.com", StoreId = "store-1", PasswordHash = "somehash" };

        _userManagerMock.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);

        var store = new Store { Id = "store-1", Url = "https://store.test", Email = "store@test.com" };

        _storeServiceMock.Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([store]);

        _notificationSearchServiceMock
            .Setup(x => x.SearchNotificationsAsync(It.IsAny<NotificationSearchCriteria>()))
            .ReturnsAsync(new NotificationSearchResult
            {
                Results = [new OrganizationInviteExistingUserEmailNotification()],
                TotalCount = 1,
            });

        var service = BuildTestableService();

        // Act
        var result = await service.ResendInviteAsync(
            new ResendInviteRequest { MembershipId = "m1", UrlSuffix = "/organization/invite" },
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.Succeeded);

        _notificationSenderMock.Verify(
            x => x.ScheduleSendNotificationAsync(It.Is<Notification>(n =>
                ((OrganizationInviteExistingUserEmailNotification)n).InviteUrl.EndsWith("/account/dashboard", StringComparison.Ordinal))),
            Times.Once);
    }

    [Fact]
    public async Task ResendInviteAsync_NewUserWithoutPassword_UsesRegistrationNotification_WithToken()
    {
        // Arrange
        var membership = new OrganizationMembership
        {
            Id = "m1",
            UserId = "user-1",
            OrganizationId = "org-1",
            Status = ModuleConstants.MembershipStatuses.Invited,
        };
        _membershipServiceMock.Setup(x => x.GetAsync(It.IsAny<IList<string>>()))
            .ReturnsAsync([membership]);

        var user = new ApplicationUser { Id = "user-1", Email = "new@test.com", StoreId = "store-1" };
        _userManagerMock.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("token123");

        var store = new Store { Id = "store-1", Url = "https://store.test", Email = "store@test.com" };
        _storeServiceMock.Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([store]);

        _notificationSearchServiceMock
            .Setup(x => x.SearchNotificationsAsync(It.IsAny<NotificationSearchCriteria>()))
            .ReturnsAsync(new NotificationSearchResult
            {
                Results = [new OrganizationInviteNewUserEmailNotification()],
                TotalCount = 1,
            });

        var service = BuildTestableService();

        // Act
        var result = await service.ResendInviteAsync(new ResendInviteRequest { MembershipId = "m1" }, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.Succeeded);
        _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(user), Times.Once);
    }

    [Fact]
    public async Task ResendInviteAsync_SsoUserWithoutPassword_UsesExistingUserNotification_NoToken()
    {
        // Arrange — an SSO-only user (Azure AD/Google) has no PasswordHash but already has a working
        // external login. They must be treated as an existing user, not sent a password-setup invite.
        var membership = new OrganizationMembership
        {
            Id = "m1",
            UserId = "user-1",
            OrganizationId = "org-1",
            Status = ModuleConstants.MembershipStatuses.Invited,
        };

        _membershipServiceMock.Setup(x => x.GetAsync(It.IsAny<IList<string>>()))
            .ReturnsAsync([membership]);

        var user = new ApplicationUser { Id = "user-1", Email = "sso@test.com", StoreId = "store-1" };

        _userManagerMock.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetLoginsAsync(user))
            .ReturnsAsync([new("AzureAD", "external-id-1", "Azure AD")]);

        var store = new Store { Id = "store-1", Url = "https://store.test", Email = "store@test.com" };

        _storeServiceMock.Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([store]);

        _notificationSearchServiceMock
            .Setup(x => x.SearchNotificationsAsync(It.IsAny<NotificationSearchCriteria>()))
            .ReturnsAsync(new NotificationSearchResult
            {
                Results = [new OrganizationInviteExistingUserEmailNotification()],
                TotalCount = 1,
            });

        var service = BuildTestableService();

        // Act
        var result = await service.ResendInviteAsync(new ResendInviteRequest { MembershipId = "m1" }, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.Succeeded);

        _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task ResendInviteAsync_UrlSuffixWithQuote_ReturnsInvalidUrlSuffixError()
    {
        // Arrange — a urlSuffix containing a quote could break out of the rendered <a href="..."> attribute.
        var membership = new OrganizationMembership
        {
            Id = "m1",
            UserId = "user-1",
            OrganizationId = "org-1",
            Status = ModuleConstants.MembershipStatuses.Invited,
        };

        _membershipServiceMock.Setup(x => x.GetAsync(It.IsAny<IList<string>>()))
            .ReturnsAsync([membership]);

        var user = new ApplicationUser { Id = "user-1", Email = "new@test.com", StoreId = "store-1" };

        _userManagerMock.Setup(x => x.FindByIdAsync("user-1")).ReturnsAsync(user);

        var store = new Store { Id = "store-1", Url = "https://store.test", Email = "store@test.com" };

        _storeServiceMock.Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([store]);

        _notificationSearchServiceMock
            .Setup(x => x.SearchNotificationsAsync(It.IsAny<NotificationSearchCriteria>()))
            .ReturnsAsync(new NotificationSearchResult
            {
                Results = [new OrganizationInviteNewUserEmailNotification()],
                TotalCount = 1,
            });

        var service = BuildTestableService();

        // Act
        var result = await service.ResendInviteAsync(
            new ResendInviteRequest { MembershipId = "m1", UrlSuffix = "/confirm\" onclick=\"alert(1)" },
            TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "InvalidUrlSuffix");

        _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task InviteCustomerAsyc_UnrelatedDbUpdateException_PropagatesInsteadOfBeingReportedAsDuplicate()
    {
        // Arrange — a DbUpdateException unrelated to a unique-index race (e.g. a transient connection drop)
        // must surface as a real failure, not be silently reported as "already a member of organization".
        var store = new Store { Id = "store-1", Url = "https://store.test", Email = "store@test.com" };

        _storeServiceMock.Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([store]);

        _notificationSearchServiceMock
            .Setup(x => x.SearchNotificationsAsync(It.IsAny<NotificationSearchCriteria>()))
            .ReturnsAsync(new NotificationSearchResult
            {
                Results = [new OrganizationInviteExistingUserEmailNotification()],
                TotalCount = 1,
            });

        var existingUser = new ApplicationUser { Id = "user-1", Email = "existing@test.com", MemberId = "contact-1" };

        _userManagerMock.Setup(x => x.FindByEmailAsync("existing@test.com")).ReturnsAsync(existingUser);

        _membershipSearchServiceMock
            .Setup(x => x.SearchAsync(
                It.Is<OrganizationMembershipSearchCriteria>(c => c.UserId == "user-1" && c.OrganizationId == "org-1"),
                It.IsAny<bool>()))
            .ReturnsAsync(new OrganizationMembershipSearchResult { Results = [] });

        _memberServiceMock.Setup(x => x.GetByIdAsync("contact-1", null, null))
            .ReturnsAsync(new Contact { Id = "contact-1", Organizations = [] });

        _membershipServiceMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<IList<OrganizationMembership>>()))
            .ThrowsAsync(new DbUpdateException("connection dropped"));

        var service = BuildTestableService();

        var request = new InviteCustomerRequest
        {
            StoreId = "store-1",
            OrganizationId = "org-1",
            Emails = ["existing@test.com"],
        };

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(
            () => service.InviteCustomerAsyc(request, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RevokeInviteAsync_PendingInvite_SetsStatusDeleted_DoesNotDeleteMembership()
    {
        // Arrange
        var membership = new OrganizationMembership
        {
            Id = "m1",
            UserId = "user-1",
            OrganizationId = "org-1",
            Status = ModuleConstants.MembershipStatuses.Invited,
        };
        _membershipServiceMock.Setup(x => x.GetAsync(It.IsAny<IList<string>>()))
            .ReturnsAsync([membership]);

        var service = BuildTestableService();

        // Act
        var result = await service.RevokeInviteAsync("m1", TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.Succeeded);
        _membershipServiceMock.Verify(x => x.DeleteAsync(It.IsAny<IList<string>>()), Times.Never);
        _membershipServiceMock.Verify(x => x.SetStatusAsync("m1", ModuleConstants.MembershipStatuses.Deleted), Times.Once);
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
            _membershipSearchServiceMock.Object,
            _companyMemberRoleServiceMock.Object,
            _loggerMock.Object);

    private sealed class TestableInviteCustomerService(
        IMemberService memberService,
        IStoreService storeService,
        INotificationSearchService notificationSearchService,
        INotificationSender notificationSender,
        Func<UserManager<ApplicationUser>> userManagerFactory,
        Func<RoleManager<Role>> roleManagerFactory,
        IOrganizationMembershipService organizationMembershipService,
        IOrganizationMembershipSearchService organizationMembershipSearchService,
        ICompanyMemberRoleService companyMemberRoleService,
        ILogger<InviteCustomerService> logger)
        : InviteCustomerService(memberService, storeService, notificationSearchService, notificationSender,
            userManagerFactory, roleManagerFactory, organizationMembershipService, organizationMembershipSearchService,
            companyMemberRoleService, logger)
    {
        public ApplicationUser InvokeCreateUser(
            InviteCustomerRequest request, Contact contact, string email, List<Role> roles)
            => CreateUser(request, contact, email, roles);

        public Task InvokeCreateOrganizationMembershipAsync(
            ApplicationUser user, string organizationId, List<Role> roles)
            => CreateOrganizationMembershipAsync(user, organizationId, roles);
    }
}
