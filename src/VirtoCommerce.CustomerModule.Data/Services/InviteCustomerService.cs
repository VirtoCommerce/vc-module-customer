using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VirtoCommerce.CustomerModule.Core.Extensions;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.NotificationsModule.Core.Extensions;
using VirtoCommerce.NotificationsModule.Core.Services;
using VirtoCommerce.NotificationsModule.Core.Types;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;

namespace VirtoCommerce.CustomerModule.Data.Services;

public class InviteCustomerService : IInviteCustomerService
{
    public virtual string InitialUserType => "Customer";
    public virtual string InitialContactStatus => "Invited";
    public virtual string[] DefaultRoleIds => new[] { "org-employee", "purchasing-agent", "org-maintainer" };

    public virtual string InvitationUrlSuffix => "/confirm-invitation";

    private readonly IMemberService _memberService;
    private readonly IStoreService _storeService;
    private readonly INotificationSearchService _notificationSearchService;
    private readonly INotificationSender _notificationSender;
    private readonly Func<UserManager<ApplicationUser>> _userManagerFactory;
    private readonly Func<RoleManager<Role>> _roleManagerFactory;
    private readonly ILogger<InviteCustomerService> _logger;

    public InviteCustomerService(
        IMemberService memberService,
        IStoreService storeService,
        INotificationSearchService notificationSearchService,
        INotificationSender notificationSender,
        Func<UserManager<ApplicationUser>> userManagerFactory,
        Func<RoleManager<Role>> roleManagerFactory,
        ILogger<InviteCustomerService> logger)
    {
        _memberService = memberService;
        _storeService = storeService;
        _notificationSearchService = notificationSearchService;
        _notificationSender = notificationSender;
        _userManagerFactory = userManagerFactory;
        _roleManagerFactory = roleManagerFactory;
        _logger = logger;
    }

    public async Task<InviteCustomerResult> InviteCustomerAsyc(InviteCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var result = new InviteCustomerResult
        {
            Errors = new List<InviteCustomerError>(),
        };

        // request validation
        if (request == null || request.Emails.IsNullOrEmpty())
        {
            result.Errors.Add(new InviteCustomerError
            {
                Code = "InvalidRequest",
                Description = "Request or Emails list is empty",
            });

            return result;
        }

        // store validation
        var store = await _storeService.GetByIdAsync(request.StoreId);
        if (store == null)
        {
            result.Errors.Add(new InviteCustomerError
            {
                Code = "StoreNotFound",
                Description = $"Store '{request.StoreId}' not found",
                Parameter = request.StoreId,
            });

            return result;
        }
        else if (string.IsNullOrEmpty(store.Url) || string.IsNullOrEmpty(store.Email))
        {
            result.Errors.Add(new InviteCustomerError
            {
                Code = "StoreNotConfigured",
                Description = $"Store '{request.StoreId}' has invalid URL or email",
                Parameter = request.StoreId,
            });

            return result;
        }

        // roles validation
        var rolesResult = await GetRolesAsync(request.RoleIds);
        if (rolesResult.Errors.Count != 0)
        {
            result.Errors.AddRange(rolesResult.Errors);

            return result;
        }

        var roleNames = rolesResult.Roles.Select(x => x.NormalizedName).ToArray();

        // notification validation
        var notificationResult = await TryGetNotification(request, store);
        if (notificationResult.Errors.Count != 0)
        {
            result.Errors.AddRange(notificationResult.Errors);

            return result;
        }

        using var userManager = _userManagerFactory();
        foreach (var email in request.Emails.Distinct())
        {
            var existingUser = await userManager.FindByEmailAsync(email) ?? await userManager.FindByNameAsync(email);
            if (existingUser != null)
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

            var contact = CreateContact(request, email);
            await _memberService.SaveChangesAsync([contact]);

            var user = CreateUser(request, contact, email, rolesResult.Roles);
            var identityResult = await userManager.CreateAsync(user);

            if (identityResult.Succeeded)
            {
                var notificationErrors = await SendNotificationAsync(notificationResult.Notification, userManager, user, request, store);
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

    public async Task<IList<CustomerRole>> GetInviteRolesAsync()
    {
        using var roleManager = _roleManagerFactory();

        var rolesQuery = roleManager.Roles.Where(x => DefaultRoleIds.Contains(x.Id));
        var roles = await rolesQuery.ToListAsync();

        var customerRoles = roles.Select(MapCustomerRole).ToList();
        return customerRoles;
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
        user.Roles = roles.ToList();

        return user;
    }

    protected virtual async Task<RolesResult> GetRolesAsync(string[] roleIds)
    {
        var result = new RolesResult();

        if (roleIds.IsNullOrEmpty())
        {
            return result;
        }

        using var roleManager = _roleManagerFactory();

        foreach (var roleId in roleIds)
        {
            var role = await roleManager.FindByIdAsync(roleId) ?? await roleManager.FindByNameAsync(roleId);
            if (role != null)
            {

                result.Roles.Add(role);
            }
            else
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
        }

        return result;
    }

    protected virtual async Task<List<InviteCustomerError>> AssignUserRoles(UserManager<ApplicationUser> userManager, ApplicationUser user, string[] roleNames)
    {
        var errors = new List<InviteCustomerError>();

        if (roleNames.IsNullOrEmpty())
        {
            return errors;
        }

        var assignResult = await userManager.AddToRolesAsync(user, roleNames);

        errors.AddRange(assignResult.Errors.Select(x => MapInviteCustomerErrorError(x, user.UserName)));

        return errors;
    }

    protected virtual async Task<NotificationResult> TryGetNotification(InviteCustomerRequest request, Store store)
    {
        var result = new NotificationResult();

        // take notification
        var notificationType = !string.IsNullOrEmpty(request.OrganizationId)
            ? typeof(RegistrationInvitationEmailNotification).Name
            : typeof(RegistrationInvitationCustomerEmailNotification).Name;

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
        Store store)
    {
        try
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            var urlSuffix = string.IsNullOrEmpty(request.UrlSuffix) ? InvitationUrlSuffix : request.UrlSuffix;

            notification.InviteUrl = $"{store.Url.TrimLastSlash()}{urlSuffix.NormalizeUrlSuffix()}?userId={user.Id}&email={HttpUtility.UrlEncode(user.Email)}&token={Uri.EscapeDataString(token)}";

            AddAdditionalParams(request, notification);

            notification.Message = request.Message;
            notification.To = user.Email;
            notification.From = store.Email;
            notification.LanguageCode = request.CultureName ?? store.DefaultLanguage;

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
