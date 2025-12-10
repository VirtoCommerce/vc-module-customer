using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Pipelines.Sockets.Unofficial.Arenas;
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

    private readonly IMemberService _memberService;
    private readonly IStoreService _storeService;
    private readonly INotificationSearchService _notificationSearchService;
    private readonly INotificationSender _notificationSender;
    private readonly IWebHostEnvironment _environment;
    private readonly Func<UserManager<ApplicationUser>> _userManagerFactory;
    private readonly Func<RoleManager<Role>> _roleManagerFactory;

    public InviteCustomerService(
        IMemberService memberService,
        IStoreService storeService,
        INotificationSearchService notificationSearchService,
        INotificationSender notificationSender,
        IWebHostEnvironment environment,
        Func<UserManager<ApplicationUser>> userManagerFactory,
        Func<RoleManager<Role>> roleManagerFactory)
    {
        _memberService = memberService;
        _storeService = storeService;
        _notificationSearchService = notificationSearchService;
        _notificationSender = notificationSender;
        _environment = environment;
        _userManagerFactory = userManagerFactory;
        _roleManagerFactory = roleManagerFactory;
    }

    public async Task<InviteCustomerResult> InviteCustomerAsyc(InviteCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var result = new InviteCustomerResult
        {
            Errors = new List<InviteCustomerError>(),
        };

        using var userManager = _userManagerFactory();

        foreach (var email in request.Emails.Distinct())
        {
            var contact = CreateContact(request, email);

            await _memberService.SaveChangesAsync([contact]);

            var user = CreateUser(request, contact, email);
            var identityResult = await userManager.CreateAsync(user);

            if (identityResult.Succeeded)
            {
                var store = await _storeService.GetByIdAsync(user.StoreId);
                if (store == null)
                {
                    var errors = _environment.IsDevelopment() ? new[] { new IdentityError { Code = "StoreNotFound", Description = "Store not found" } } : null;
                    identityResult = IdentityResult.Failed(errors);
                }
                else
                {
                    if (string.IsNullOrEmpty(store.Url) || string.IsNullOrEmpty(store.Email))
                    {
                        var errors = _environment.IsDevelopment() ? new[] { new IdentityError { Code = "StoreNotConfigured", Description = "Store has invalid URL or email" } } : null;
                        identityResult = IdentityResult.Failed(errors);
                    }
                    else
                    {
                        result.Errors.AddRange(await AssignUserRoles(user, request.RoleIds));
                        await SendNotificationAsync(request, store, email);
                    }
                }
            }

            result.Errors.AddRange(identityResult.Errors.Select(MapInviteCustomerErrorError));

            if (!identityResult.Succeeded)
            {
                await _memberService.DeleteAsync(new[] { contact.Id });

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

    protected virtual ApplicationUser CreateUser(InviteCustomerRequest request, Contact contact, string email)
    {
        var user = AbstractTypeFactory<ApplicationUser>.TryCreateInstance();

        user.UserName = email;
        user.Email = email;
        user.MemberId = contact.Id;
        user.StoreId = request.StoreId;
        user.UserType = InitialUserType;
        user.LockoutEnd = DateTimeOffset.MaxValue;

        return user;
    }

    protected virtual async Task<List<InviteCustomerError>> AssignUserRoles(ApplicationUser user, string[] roleIds)
    {
        var errors = new List<InviteCustomerError>();
        var roles = new List<Role>();

        if (roleIds.IsNullOrEmpty())
        {
            return errors;
        }

        using var roleManager = _roleManagerFactory();
        using var userManager = _userManagerFactory();

        foreach (var roleId in roleIds)
        {
            var role = await roleManager.FindByIdAsync(roleId) ?? await roleManager.FindByNameAsync(roleId);
            if (role != null)
            {
                roles.Add(role);
            }
            else
            {
                errors.Add(new InviteCustomerError { Code = "Role not found", Description = $"Role '{roleId}' not found", Parameter = roleId });
            }
        }

        var assignResult = await userManager.AddToRolesAsync(user, roles.Select(x => x.NormalizedName).ToArray());
        errors.AddRange(assignResult.Errors.Select(MapInviteCustomerErrorError));

        return errors;
    }

    protected virtual async Task SendNotificationAsync(InviteCustomerRequest request, Store store, string email)
    {
        using var userManager = _userManagerFactory();

        var user = await userManager.FindByEmailAsync(email);
        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        // take notification
        RegistrationInvitationNotificationBase notification = !string.IsNullOrEmpty(request.OrganizationId)
            ? await _notificationSearchService.GetNotificationAsync<RegistrationInvitationEmailNotification>(new TenantIdentity(store.Id, nameof(Store)))
            : await _notificationSearchService.GetNotificationAsync<RegistrationInvitationCustomerEmailNotification>(new TenantIdentity(store.Id, nameof(Store)));

        notification.InviteUrl = $"{store.Url.TrimLastSlash()}{request.UrlSuffix.NormalizeUrlSuffix()}?userId={user.Id}&email={HttpUtility.UrlEncode(user.Email)}&token={Uri.EscapeDataString(token)}";

        AddAdditionalParams(request, notification);

        notification.Message = request.Message;
        notification.To = user.Email;
        notification.From = store.Email;
        notification.LanguageCode = request.CultureName ?? store.DefaultLanguage;

        await _notificationSender.ScheduleSendNotificationAsync(notification);
    }

    protected virtual void AddAdditionalParams(InviteCustomerRequest request, RegistrationInvitationNotificationBase notification)
    {
        if (request.AdditionalParameters == null)
        {
            return;
        }

        foreach (var param in request.AdditionalParameters)
        {
            if (!param.Value.IsNullOrEmpty())
            {
                notification.InviteUrl = $"{notification.InviteUrl}&{param.Key}={param.Value}";
            }
        }
    }

    protected virtual InviteCustomerError MapInviteCustomerErrorError(IdentityError error)
    {
        var result = AbstractTypeFactory<InviteCustomerError>.TryCreateInstance();

        if (error is CustomIdentityError customIdentityError)
        {
            result.Parameter = customIdentityError.Parameter?.ToString();
        }

        result.Code = error.Code;
        result.Description = error.Description;

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
}
