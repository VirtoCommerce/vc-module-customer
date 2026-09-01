using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VirtoCommerce.CustomerModule.Core;
using VirtoCommerce.CustomerModule.Core.Extensions;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Notifications;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.NotificationsModule.Core.Extensions;
using VirtoCommerce.NotificationsModule.Core.Services;
using VirtoCommerce.NotificationsModule.Core.Types;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Extensions;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CustomerModule.Data.Services;

public class InviteCustomerService : IInviteCustomerService
{
    public virtual string InitialUserType => "Customer";
    public virtual string InitialContactStatus => "Invited";

    public virtual string InvitationUrlSuffix => "/confirm-invitation";

    public virtual string ExistingUserInviteUrlSuffix => "/account/dashboard";

    private readonly IMemberService _memberService;
    private readonly IStoreService _storeService;
    private readonly INotificationSearchService _notificationSearchService;
    private readonly INotificationSender _notificationSender;
    private readonly Func<UserManager<ApplicationUser>> _userManagerFactory;
    private readonly Func<RoleManager<Role>> _roleManagerFactory;
    private readonly IOrganizationMembershipService _organizationMembershipService;
    private readonly IOrganizationMembershipSearchService _organizationMembershipSearchService;
    private readonly ICompanyMemberRoleService _companyMemberRoleService;
    private readonly ILogger<InviteCustomerService> _logger;

    public InviteCustomerService(
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
    {
        _memberService = memberService;
        _storeService = storeService;
        _notificationSearchService = notificationSearchService;
        _notificationSender = notificationSender;
        _userManagerFactory = userManagerFactory;
        _roleManagerFactory = roleManagerFactory;
        _organizationMembershipService = organizationMembershipService;
        _organizationMembershipSearchService = organizationMembershipSearchService;
        _companyMemberRoleService = companyMemberRoleService;
        _logger = logger;
    }

    public async Task<InviteCustomerResult> InviteCustomerAsyc(InviteCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var result = new InviteCustomerResult
        {
            Errors = new List<InviteCustomerError>(),
        };

        if (request == null || request.Emails.IsNullOrEmpty())
        {
            result.Errors.Add(new InviteCustomerError
            {
                Code = "InvalidRequest",
                Description = "Request or Emails list is empty",
            });

            return result;
        }

        var storeResult = await GetStoreAsync(request.StoreId);
        if (storeResult.Errors.Count != 0)
        {
            result.Errors.AddRange(storeResult.Errors);

            return result;
        }

        var store = storeResult.Store;

        var rolesResult = await GetRolesAsync(request.RoleIds, request.StoreId, request.OrganizationId);
        if (rolesResult.Errors.Count != 0)
        {
            result.Errors.AddRange(rolesResult.Errors);

            return result;
        }

        var notificationResult = await TryGetNotification(GetNewUserNotificationType(request), store);
        if (notificationResult.Errors.Count != 0)
        {
            result.Errors.AddRange(notificationResult.Errors);

            return result;
        }

        NotificationResult existingUserNotificationResult = null;
        if (!string.IsNullOrEmpty(request.OrganizationId))
        {
            existingUserNotificationResult = await TryGetNotification(
                typeof(OrganizationInviteExistingUserEmailNotification).Name, store);

            if (existingUserNotificationResult.Errors.Count != 0)
            {
                result.Errors.AddRange(existingUserNotificationResult.Errors);

                return result;
            }
        }

        var organizationName = string.IsNullOrEmpty(request.OrganizationId) ? null : await GetOrganizationName(request.OrganizationId);

        using var userManager = _userManagerFactory();
        foreach (var email in request.Emails.Distinct())
        {
            var existingUser = await userManager.FindByEmailAsync(email) ?? await userManager.FindByNameAsync(email);
            if (existingUser != null)
            {
                if (string.IsNullOrEmpty(request.OrganizationId))
                {
                    result.Errors.Add(new InviteCustomerError
                    {
                        Code = "UserAlreadyExists",
                        Description = $"User with email '{email}' already exists",
                        Parameter = email,
                        Email = email,
                    });

                    continue;
                }

                var membershipErrors = await InviteExistingUserToOrganization(
                    existingUser, request, rolesResult.Roles, existingUserNotificationResult?.Notification, store, organizationName);

                result.Errors.AddRange(membershipErrors);
                result.Succeeded |= membershipErrors.Count == 0;

                continue;
            }

            var contact = CreateContact(request, email);
            await _memberService.SaveChangesAsync([contact]);

            var user = CreateUser(request, contact, email, rolesResult.Roles);
            var identityResult = await userManager.CreateAsync(user);

            if (identityResult.Succeeded)
            {
                if (!string.IsNullOrEmpty(request.OrganizationId))
                {
                    await CreateOrganizationMembershipAsync(
                        user, request.OrganizationId, rolesResult.Roles, ModuleConstants.MembershipStatuses.Invited);
                }

                var notificationErrors = await SendNotificationAsync(notificationResult.Notification, userManager, user, request, store, organizationName);
                if (notificationErrors.Count != 0)
                {
                    identityResult = IdentityResult.Failed();
                    result.Errors.AddRange(notificationErrors);
                }
            }

            result.Errors.AddRange(identityResult.Errors.Select(x => MapInviteCustomerErrorError(x, user.UserName)));
            result.Succeeded |= identityResult.Succeeded;

            if (!identityResult.Succeeded)
            {
                await _memberService.DeleteAsync([contact.Id]);

                if (user.Id != null)
                {
                    await userManager.DeleteAsync(user);
                }
            }
        }

        return result;
    }

    public virtual async Task<InviteCustomerResult> RevokeInviteAsync(string membershipId, CancellationToken cancellationToken = default)
    {
        var result = new InviteCustomerResult { Errors = new List<InviteCustomerError>() };

        var membership = await GetPendingInvite(membershipId, result);
        if (membership == null)
        {
            return result;
        }

        await _organizationMembershipService.SetStatusAsync(membership.Id, ModuleConstants.MembershipStatuses.Deleted);

        result.Succeeded = true;

        return result;
    }

    public virtual async Task<InviteCustomerResult> ResendInviteAsync(ResendInviteRequest request, CancellationToken cancellationToken = default)
    {
        var result = new InviteCustomerResult { Errors = new List<InviteCustomerError>() };

        var membership = await GetPendingInvite(request.MembershipId, result);
        if (membership == null)
        {
            return result;
        }

        using var userManager = _userManagerFactory();
        var user = await userManager.FindByIdAsync(membership.UserId);
        if (user == null)
        {
            result.Errors.Add(new InviteCustomerError { Code = "UserNotFound", Description = "Invited user not found" });
            return result;
        }

        var storeResult = await GetStoreAsync(user.StoreId);
        if (storeResult.Errors.Count != 0)
        {
            result.Errors.AddRange(storeResult.Errors);
            return result;
        }

        var inviteRequest = new InviteCustomerRequest
        {
            StoreId = user.StoreId,
            OrganizationId = membership.OrganizationId,
            UrlSuffix = request.UrlSuffix,
            Message = request.Message,
        };

        var isExistingUser = !string.IsNullOrEmpty(user.PasswordHash) || (await userManager.GetLoginsAsync(user)).Count > 0;

        var notificationResult = await TryGetNotification(
            isExistingUser ? typeof(OrganizationInviteExistingUserEmailNotification).Name : GetNewUserNotificationType(inviteRequest),
            storeResult.Store);

        if (notificationResult.Errors.Count != 0)
        {
            result.Errors.AddRange(notificationResult.Errors);
            return result;
        }

        var organizationName = string.IsNullOrEmpty(inviteRequest.OrganizationId) ? null : await GetOrganizationName(inviteRequest.OrganizationId);

        var notificationErrors = isExistingUser
            ? await SendExistingUserInviteNotification(notificationResult.Notification, user, inviteRequest, storeResult.Store, organizationName)
            : await SendNotificationAsync(notificationResult.Notification, userManager, user, inviteRequest, storeResult.Store, organizationName);

        result.Errors.AddRange(notificationErrors);
        result.Succeeded = notificationErrors.Count == 0;

        return result;
    }

    public async Task<IList<CustomerRole>> GetInviteRolesAsync()
    {
        var allowedRoleIds = await _companyMemberRoleService.GetAllowedRoleIdsAsync(null);

        using var roleManager = _roleManagerFactory();
        var roles = await roleManager.Roles.ToListAsync();

        var customerRoles = roles
            .Where(allowedRoleIds.IsRoleAllowed)
            .Select(MapCustomerRole)
            .ToList();
        return customerRoles;
    }

    protected virtual async Task<OrganizationMembership> GetPendingInvite(string membershipId, InviteCustomerResult result)
    {
        var membership = (await _organizationMembershipService.GetAsync([membershipId])).FirstOrDefault();
        if (membership == null || membership.Status != ModuleConstants.MembershipStatuses.Invited)
        {
            result.Errors.Add(new InviteCustomerError
            {
                Code = "InviteNotFound",
                Description = "Pending invite not found",
                Parameter = membershipId,
            });

            return null;
        }

        return membership;
    }

    protected virtual Contact CreateContact(InviteCustomerRequest request, string email)
    {
        var contact = AbstractTypeFactory<Contact>.TryCreateInstance();

        contact.Status = InitialContactStatus;
        contact.FirstName = string.Empty;
        contact.LastName = string.Empty;
        contact.FullName = string.Empty;
        contact.Emails = new List<string> { email };

        if (!string.IsNullOrEmpty(request.OrganizationId))
        {
            contact.Organizations = new List<string> { request.OrganizationId };
        }

        return contact;
    }

    protected virtual ApplicationUser CreateUser(InviteCustomerRequest request, Contact contact, string email, List<Role> roles)
    {
        var user = AbstractTypeFactory<ApplicationUser>.TryCreateInstance();

        user.UserName = email;
        user.Email = email;
        user.MemberId = contact.Id;
        user.StoreId = request.StoreId;
        user.UserType = InitialUserType;
        user.LockoutEnd = DateTimeOffset.MaxValue;

        // Assign roles globally only when there's no organization context.
        // With an organization, roles go into OrganizationMembership instead.
        if (string.IsNullOrEmpty(request.OrganizationId))
        {
            user.Roles = roles.ToList();
        }

        return user;
    }

    protected virtual async Task<List<InviteCustomerError>> InviteExistingUserToOrganization(
        ApplicationUser existingUser,
        InviteCustomerRequest request,
        List<Role> roles,
        RegistrationInvitationNotificationBase notification,
        Store store,
        string organizationName)
    {
        var existingMembership = await _organizationMembershipSearchService.GetMembershipAsync(existingUser.Id, request.OrganizationId);

        if (existingMembership != null)
        {
            if (!ModuleConstants.MembershipStatuses.ReinvitableStatuses.Contains(existingMembership.Status))
            {
                return [CreateAlreadyMemberOfOrganizationError(existingUser.Email, request.OrganizationId)];
            }

            await ReinviteExistingMembership(existingMembership, roles);
        }
        else
        {
            try
            {
                await CreateOrganizationMembershipAsync(existingUser, request.OrganizationId, roles, ModuleConstants.MembershipStatuses.Invited);
            }
            catch (Exception ex) when (IsDuplicateMembershipException(ex))
            {
                return [CreateAlreadyMemberOfOrganizationError(existingUser.Email, request.OrganizationId)];
            }
        }

        await AddOrganizationToContact(existingUser.MemberId, request.OrganizationId);

        return await SendExistingUserInviteNotification(notification, existingUser, request, store, organizationName);
    }

    protected virtual async Task<List<InviteCustomerError>> SendExistingUserInviteNotification(
        RegistrationInvitationNotificationBase notification, ApplicationUser existingUser, InviteCustomerRequest request, Store store, string organizationName)
    {
        try
        {
            var urlSuffix = ExistingUserInviteUrlSuffix;
            if (!IsValidUrlSuffix(urlSuffix))
            {
                return
                [
                    new InviteCustomerError
                    {
                        Code = "InvalidUrlSuffix",
                        Description = "UrlSuffix must be a relative path.",
                        Parameter = urlSuffix,
                    }
                ];
            }

            notification.InviteUrl = $"{store.Url.TrimLastSlash()}{urlSuffix.NormalizeUrlSuffix()}";

            AddAdditionalParams(request, notification);

            notification.Message = request.Message;
            notification.To = existingUser.Email;
            notification.From = store.Email;
            notification.LanguageCode = request.CultureName ?? store.DefaultLanguage;

            if (notification is OrganizationInviteExistingUserEmailNotification existingUserNotification)
            {
                existingUserNotification.OrganizationName = organizationName;
                existingUserNotification.CustomerName = await GetContactName(existingUser.MemberId);
            }

            await _notificationSender.ScheduleSendNotificationAsync(notification);

            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending invitation notification to '{email}'", existingUser.Email);

            return
            [
                new InviteCustomerError
                {
                    Code = "NotificationSendError",
                    Description = $"Error sending invitation notification to '{existingUser.Email}'. Check Notification Feed.",
                    Email = existingUser.Email,
                }
            ];
        }
    }

    protected virtual async Task AddOrganizationToContact(string memberId, string organizationId)
    {
        if (string.IsNullOrEmpty(memberId))
        {
            return;
        }

        if (await _memberService.GetByIdAsync(memberId) is not Contact contact)
        {
            return;
        }

        contact.Organizations ??= [];

        if (!contact.Organizations.Contains(organizationId, StringComparer.OrdinalIgnoreCase))
        {
            contact.Organizations.Add(organizationId);
            await _memberService.SaveChangesAsync([contact]);
        }
    }

    protected virtual async Task CreateOrganizationMembershipAsync(ApplicationUser user, string organizationId, List<Role> roles, string status = null)
    {
        var membership = AbstractTypeFactory<OrganizationMembership>.TryCreateInstance();
        membership.UserId = user.Id;
        membership.OrganizationId = organizationId;
        membership.Status = status;
        membership.Roles = BuildMembershipRoles(roles);

        await _organizationMembershipService.SaveChangesAsync([membership]);
    }

    protected virtual async Task ReinviteExistingMembership(OrganizationMembership membership, List<Role> roles)
    {
        membership.Status = ModuleConstants.MembershipStatuses.Invited;
        membership.IsLocked = false;
        membership.LockoutEnd = null;
        membership.Roles = BuildMembershipRoles(roles);

        await _organizationMembershipService.SaveChangesAsync([membership]);
    }

    protected virtual List<OrganizationMembershipRole> BuildMembershipRoles(List<Role> roles)
    {
        return roles
            .Select(r =>
            {
                var membershipRole = AbstractTypeFactory<OrganizationMembershipRole>.TryCreateInstance();
                membershipRole.RoleId = r.Id;
                membershipRole.RoleName = r.Name;
                return membershipRole;
            })
            .ToList();
    }

    protected virtual async Task<StoreResult> GetStoreAsync(string storeId)
    {
        var result = new StoreResult();

        var store = await _storeService.GetByIdAsync(storeId);
        if (store == null)
        {
            result.Errors.Add(new InviteCustomerError
            {
                Code = "StoreNotFound",
                Description = $"Store '{storeId}' not found",
                Parameter = storeId,
            });

            return result;
        }
        else if (string.IsNullOrEmpty(store.Url) || string.IsNullOrEmpty(store.Email))
        {
            result.Errors.Add(new InviteCustomerError
            {
                Code = "StoreNotConfigured",
                Description = $"Store '{storeId}' has invalid URL or email",
                Parameter = storeId,
            });

            return result;
        }

        result.Store = store;

        return result;
    }

    protected virtual async Task<RolesResult> GetRolesAsync(string[] roleIds, string storeId, string organizationId)
    {
        var result = new RolesResult();

        if (roleIds.IsNullOrEmpty())
        {
            return result;
        }

        IList<string> allowedRoleIds = null;
        if (!string.IsNullOrEmpty(organizationId))
        {
            allowedRoleIds = await _companyMemberRoleService.GetAllowedRoleIdsAsync(storeId);
        }

        using var roleManager = _roleManagerFactory();

        foreach (var roleId in roleIds)
        {
            var role = await roleManager.FindByIdAsync(roleId) ?? await roleManager.FindByNameAsync(roleId);
            if (role == null)
            {
                result.Errors.Add(new InviteCustomerError
                {
                    Code = "Role not found",
                    Description = $"Role '{roleId}' not found",
                    Parameter = roleId,
                    Email = "Common",
                });

                return result;
            }

            if (allowedRoleIds != null && !allowedRoleIds.IsRoleAllowed(role))
            {
                result.Errors.Add(new InviteCustomerError
                {
                    Code = "RoleNotAllowed",
                    Description = $"Role '{roleId}' is not allowed for company members of this store",
                    Parameter = roleId,
                    Email = "Common",
                });

                return result;
            }

            result.Roles.Add(role);
        }

        return result;
    }

    protected static string GetNewUserNotificationType(InviteCustomerRequest request) =>
        !string.IsNullOrEmpty(request.OrganizationId)
            ? typeof(OrganizationInviteNewUserEmailNotification).Name
            : typeof(CustomerInviteNewUserEmailNotification).Name;

    protected virtual async Task<NotificationResult> TryGetNotification(string notificationType, Store store)
    {
        var result = new NotificationResult();

        var notification = await _notificationSearchService.GetNotificationAsync(notificationType, new TenantIdentity(store.Id, nameof(Store)));
        var registrationNotification = notification?.Clone() as RegistrationInvitationNotificationBase;

        if (registrationNotification == null)
        {
            result.Errors.Add(new InviteCustomerError
            {
                Code = "NotificationNotFound",
                Description = "Notification not found",
                Parameter = notificationType,
            });
            return result;
        }

        result.Notification = registrationNotification;

        return result;
    }

    protected virtual async Task<List<InviteCustomerError>> SendNotificationAsync(RegistrationInvitationNotificationBase notification,
        UserManager<ApplicationUser> userManager,
        ApplicationUser user,
        InviteCustomerRequest request,
        Store store,
        string organizationName)
    {
        try
        {
            var urlSuffix = string.IsNullOrEmpty(request.UrlSuffix) ? InvitationUrlSuffix : request.UrlSuffix;
            if (!IsValidUrlSuffix(urlSuffix))
            {
                return
                [
                    new InviteCustomerError
                    {
                        Code = "InvalidUrlSuffix",
                        Description = "UrlSuffix must be a relative path.",
                        Parameter = urlSuffix,
                    }
                ];
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            notification.InviteUrl = $"{store.Url.TrimLastSlash()}{urlSuffix.NormalizeUrlSuffix()}?userId={user.Id}&email={HttpUtility.UrlEncode(user.Email)}&token={Uri.EscapeDataString(token)}";

            if (!string.IsNullOrEmpty(request.OrganizationId))
            {
                notification.InviteUrl = $"{notification.InviteUrl}&organizationId={HttpUtility.UrlEncode(request.OrganizationId)}";
            }

            AddAdditionalParams(request, notification);

            notification.Message = request.Message;
            notification.To = user.Email;
            notification.From = store.Email;
            notification.LanguageCode = request.CultureName ?? store.DefaultLanguage;

            if (notification is OrganizationInviteNewUserEmailNotification newUserNotification)
            {
                newUserNotification.OrganizationName = organizationName;
            }

            await _notificationSender.ScheduleSendNotificationAsync(notification);

            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending invitation notification to '{email}'", user.Email);

            return
            [
                new InviteCustomerError
                {
                    Code = "NotificationSendError",
                    Description = $"Error sending invitation notification to '{user.Email}'. Check Notification Feed.",
                    Email = user.Email,
                }
            ];
        }
    }

    protected virtual async Task<string> GetOrganizationName(string organizationId)
    {
        if (string.IsNullOrEmpty(organizationId))
        {
            return null;
        }

        return (await _memberService.GetByIdAsync(organizationId, memberType: nameof(Organization)) as Organization)?.Name;
    }

    protected virtual async Task<string> GetContactName(string memberId)
    {
        if (string.IsNullOrEmpty(memberId))
        {
            return null;
        }

        return (await _memberService.GetByIdAsync(memberId) as Contact)?.FullName;
    }

    protected virtual void AddAdditionalParams(InviteCustomerRequest request, RegistrationInvitationNotificationBase notification)
    {
        if (request.AdditionalParameters == null)
        {
            return;
        }

        foreach (var param in request.AdditionalParameters.Where(param => !param.Value.IsNullOrEmpty()))
        {
            notification.InviteUrl = $"{notification.InviteUrl}&{HttpUtility.UrlEncode(param.Key)}={HttpUtility.UrlEncode(param.Value)}";
        }
    }

    protected virtual InviteCustomerError MapInviteCustomerErrorError(IdentityError error, string email = null)
    {
        var result = AbstractTypeFactory<InviteCustomerError>.TryCreateInstance();

        result.Code = error.Code;
        result.Description = error.Description;
        result.Email = email;

        if (error is CustomIdentityError customIdentityError)
        {
            result.Parameter = customIdentityError.Parameter?.ToString();
        }

        return result;
    }

    protected virtual CustomerRole MapCustomerRole(Role role)
    {
        var customerRole = AbstractTypeFactory<CustomerRole>.TryCreateInstance();

        customerRole.Id = role.Id;
        customerRole.Name = role.Name;
        customerRole.Description = role.Description;

        return customerRole;
    }

    private static InviteCustomerError CreateAlreadyMemberOfOrganizationError(string email, string organizationId) => new()
    {
        Code = "AlreadyMemberOfOrganization",
        Description = $"User with email '{email}' is already a member of organization '{organizationId}'",
        Parameter = organizationId,
        Email = email,
    };

    private static bool IsDuplicateMembershipException(Exception ex) => ex is InvalidOperationException;

    private static bool IsValidUrlSuffix(string urlSuffix)
    {
        return string.IsNullOrEmpty(urlSuffix) ||
            (!urlSuffix.StartsWith("//", StringComparison.Ordinal) &&
             urlSuffix.IndexOfAny(['"', '<', '>', ':', '\r', '\n']) < 0);
    }

    protected class StoreResult
    {
        public Store Store { get; set; }
        public List<InviteCustomerError> Errors { get; set; } = [];
    }

    protected class RolesResult
    {
        public List<Role> Roles { get; set; } = [];
        public List<InviteCustomerError> Errors { get; set; } = [];
    }

    protected class NotificationResult
    {
        public RegistrationInvitationNotificationBase Notification { get; set; }
        public List<InviteCustomerError> Errors { get; set; } = [];
    }
}
