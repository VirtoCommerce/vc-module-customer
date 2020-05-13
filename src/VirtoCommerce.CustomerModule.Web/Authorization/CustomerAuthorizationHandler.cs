using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Security.Authorization;

namespace VirtoCommerce.CustomerModule.Web.Authorization
{
    public sealed class
        CustomerAuthorizationHandler : PermissionAuthorizationHandlerBase<CustomerAuthorizationRequirement>
    {
        private readonly MvcNewtonsoftJsonOptions _jsonOptions;
        private readonly Func<UserManager<ApplicationUser>> _userManagerFactory;
        private readonly IMemberService _memberService;

        public CustomerAuthorizationHandler(IOptions<MvcNewtonsoftJsonOptions> jsonOptions,
            Func<UserManager<ApplicationUser>> userManagerFactory, IMemberService memberService)
        {
            _jsonOptions = jsonOptions.Value;
            _userManagerFactory = userManagerFactory;
            _memberService = memberService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CustomerAuthorizationRequirement requirement)
        {
            await base.HandleRequirementAsync(context, requirement);

            if (context.HasSucceeded)
            {
                return;
            }

            var userPermission = context.User.FindPermission(requirement.Permission, _jsonOptions.SerializerSettings);
            if (userPermission == null)
            {
                return;
            }

            var associatedOrganizationsOnlyScope = userPermission.AssignedScopes.OfType<AssociatedOrganizationsOnlyScope>().FirstOrDefault();
            if (associatedOrganizationsOnlyScope == null)
            {
                return;
            }

            using var userManager = _userManagerFactory();
            var currentUser = await userManager.GetUserAsync(context.User);
            if (string.IsNullOrEmpty(currentUser.MemberId))
            {
                return;
            }

            var currentContact = (Contact) await _memberService.GetByIdAsync(currentUser.MemberId);
            switch (context.Resource)
            {
                case MembersSearchCriteria criteria when !string.IsNullOrEmpty(criteria.MemberId):
                {
                    if (currentContact.AssociatedOrganizations.Contains(criteria.MemberId))
                    {
                        context.Succeed(requirement);
                    }

                    break;
                }
                case MembersSearchCriteria criteria:
                    criteria.ObjectIds = currentContact.AssociatedOrganizations;
                    context.Succeed(requirement);
                    break;
                case Member[] members:
                {
                    var filteredOrganizations = members.OfType<Organization>()
                        .Where(o => IsOrganizationInAssociatedOrganizations(currentContact, o)).ToArray();
                    var filteredContacts = members.OfType<Contact>()
                        .Where(c => IsContactHasAssociatedOrganization(currentContact, c)).ToArray();
                    if (filteredOrganizations.Any())
                    {
                        CopyMembers(ref members, filteredOrganizations);
                        context.Succeed(requirement);
                    }

                    if (filteredContacts.Any())
                    {
                        CopyMembers(ref members, filteredContacts, filteredOrganizations.Length);
                        context.Succeed(requirement);
                    }

                    break;
                }
                case Organization organization:
                {
                    if (IsOrganizationInAssociatedOrganizations(currentContact, organization))
                    {
                        context.Succeed(requirement);
                    }

                    break;
                }
                case Contact contact:
                {
                    if (IsContactHasAssociatedOrganization(currentContact, contact))
                    {
                        context.Succeed(requirement);
                    }

                    break;
                }
            }
        }

        private bool IsOrganizationInAssociatedOrganizations(Contact contact, Organization organization)
        {
            return contact.AssociatedOrganizations.Contains(organization.Id);
        }

        private bool IsContactHasAssociatedOrganization(Contact currentContact, Contact checkingContact)
        {
            return currentContact.AssociatedOrganizations.Any(o => checkingContact.Organizations.Contains(o));
        }

        private void CopyMembers<TDestination, TSource>(ref TDestination[] destination, TSource[] source, int destinationIndex = 0)
            where TDestination : Member
            where TSource : Member, TDestination
        {
            Array.Resize(ref destination, source.Length);
            Array.Copy(source, 0, destination, destinationIndex, source.Length + destinationIndex);
        }
    }
}
