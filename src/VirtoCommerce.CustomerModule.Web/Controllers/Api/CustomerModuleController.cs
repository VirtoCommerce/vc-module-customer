using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CoreModule.Core.Common;
using VirtoCommerce.CustomerModule.Core;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Web.Authorization;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using Address = VirtoCommerce.CustomerModule.Core.Model.Address;

namespace VirtoCommerce.CustomerModule.Web.Controllers.Api
{
    [Route("api")]
    [Authorize]

    public class CustomerModuleController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IMemberService _memberService;
        private readonly IMemberSearchService _memberSearchService;
        private readonly IInviteCustomerService _inviteCustomerService;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private UserManager<ApplicationUser> UserManager => _signInManager.UserManager;

        public CustomerModuleController(IAuthorizationService authorizationService,
            IMemberService memberService,
            IMemberSearchService memberSearchService,
            IInviteCustomerService inviteCustomerService,
            SignInManager<ApplicationUser> signInManager)
        {
            _authorizationService = authorizationService;
            _memberService = memberService;
            _memberSearchService = memberSearchService;
            _inviteCustomerService = inviteCustomerService;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Get organizations
        /// </summary>
        /// <remarks>Get array of all organizations.</remarks>
        [HttpGet]
        [Route("members/organizations")]
        public async Task<ActionResult<Organization[]>> ListOrganizations()
        {
            var searchCriteria = new MembersSearchCriteria
            {
                MemberType = typeof(Organization).Name,
                DeepSearch = true,
                Take = int.MaxValue
            };
            if (!(await AuthorizeAsync(searchCriteria, ModuleConstants.Security.Permissions.Access)).Succeeded)
            {
                return Forbid();
            }
            var result = await _memberSearchService.SearchMembersAsync(searchCriteria);

            return Ok(result.Results.OfType<Organization>());
        }

        /// <summary>
        /// Get members
        /// </summary>
        /// <remarks>Get array of members satisfied search criteria.</remarks>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        [HttpPost]
        [Route("members/search")]
        public async Task<ActionResult<MemberSearchResult>> SearchMember([FromBody] MembersSearchCriteria criteria)
        {
            if (!(await AuthorizeAsync(criteria, ModuleConstants.Security.Permissions.Read)).Succeeded)
            {
                return Forbid();
            }
            var result = await _memberSearchService.SearchMembersAsync(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Get member
        /// </summary>
        /// <param name="id">member id</param>
        /// <param name="responseGroup">response group</param>
        /// <param name="memberType">member type</param>
        [HttpGet]
        [Route("members/{id}")]
        public async Task<ActionResult<Member>> GetMemberById(string id, [FromQuery] string responseGroup = null, [FromQuery] string memberType = null)
        {
            //pass member type name for better perfomance
            var retVal = await _memberService.GetByIdAsync(id, responseGroup, memberType);
            if (!(await AuthorizeAsync(retVal, ModuleConstants.Security.Permissions.Read)).Succeeded)
            {
                return Forbid();
            }
            if (retVal != null)
            {
                // Casting to dynamic fixes a serialization error in XML formatter when the returned object type is derived from the Member class.
                return Ok((dynamic)retVal);
            }
            return Ok();
        }

        [HttpGet]
        [Route("members/accounts/{userId}")]
        public async Task<ActionResult<Member>> GetMemberByUserId(string userId, [FromQuery] string responseGroup = null, [FromQuery] string memberType = null)
        {
            var user = await UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            //pass member type name for better perfomance
            var retVal = await _memberService.GetByIdAsync(user.MemberId, responseGroup, memberType);
            if (!(await AuthorizeAsync(retVal, ModuleConstants.Security.Permissions.Read)).Succeeded)
            {
                return Forbid();
            }
            if (retVal != null)
            {
                // Casting to dynamic fixes a serialization error in XML formatter when the returned object type is derived from the Member class.
                return Ok((dynamic)retVal);
            }
            return Ok();
        }

        [HttpGet]
        [Route("members")]
        public async Task<ActionResult<Member[]>> GetMembersByIds([FromQuery] string[] ids, [FromQuery] string responseGroup = null, [FromQuery] string[] memberTypes = null)
        {
            //pass member types name for better performance
            var retVal = await _memberService.GetByIdsAsync(ids, responseGroup, memberTypes);
            if (!(await AuthorizeAsync(retVal, ModuleConstants.Security.Permissions.Read)).Succeeded)
            {
                return Forbid();
            }
            if (retVal != null)
            {
                // Casting to dynamic fixes a serialization error in XML formatter when the returned object type is derived from the Member class.
                return Ok(retVal.Cast<dynamic>().ToArray());
            }
            return Ok();
        }

        /// <summary>
        /// Create new member (can be any object inherited from Member type)
        /// </summary>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        /// <returns></returns>
        [HttpPost]
        [Route("members")]
        public async Task<ActionResult<Member>> CreateMember([FromBody] Member member)
        {
            if (!(await AuthorizeAsync(member, ModuleConstants.Security.Permissions.Create)).Succeeded)
            {
                return Forbid();
            }

            if (HasDefaultBillingAndShippingAddress(member, out var actionResult))
            {
                return actionResult;
            }

            await _memberService.SaveChangesAsync([member]);

            var retVal = await _memberService.GetByIdAsync(member.Id, null, member.MemberType);

            // Casting to dynamic fixes a serialization error in XML formatter when the returned object type is derived from the Member class.
            return Ok((dynamic)retVal);
        }

        /// <summary>
        /// Bulk create new members (can be any objects inherited from Member type)
        /// </summary>
        /// <param name="members">Array of concrete instances of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        /// <returns></returns>
        [HttpPost]
        [Route("members/bulk")]
        public async Task<ActionResult<Member[]>> BulkCreateMembers([FromBody] Member[] members)
        {
            if (!(await AuthorizeAsync(members, ModuleConstants.Security.Permissions.Create)).Succeeded)
            {
                return Forbid();
            }
            await _memberService.SaveChangesAsync(members);
            var retVal = await _memberService.GetByIdsAsync(members.Select(m => m.Id).ToArray(), null, members.Select(m => m.MemberType).Distinct().ToArray());

            // Casting to dynamic fixes a serialization error in XML formatter when the returned object type is derived from the Member class.
            return Ok((dynamic)retVal);
        }

        /// <summary>
        /// Update member
        /// </summary>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        [HttpPut]
        [Route("members")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdateMember([FromBody] Member member)
        {
            if (!(await AuthorizeAsync(member, ModuleConstants.Security.Permissions.Update)).Succeeded)
            {
                return Forbid();
            }

            if (HasDefaultBillingAndShippingAddress(member, out var actionResult))
            {
                return actionResult;
            }

            await _memberService.SaveChangesAsync([member]);
            return NoContent();
        }

        /// <summary>
        /// Bulk update members
        /// </summary>
        /// <param name="members">Array of concrete instances of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        [HttpPut]
        [Route("members/bulk")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> BulkUpdateMembers([FromBody] Member[] members)
        {
            if (!(await AuthorizeAsync(members, ModuleConstants.Security.Permissions.Update)).Succeeded)
            {
                return Forbid();
            }
            await _memberService.SaveChangesAsync(members);
            return NoContent();
        }

        /// <summary>
        /// Delete members
        /// </summary>
        /// <remarks>Delete members by given array of ids.</remarks>
        /// <param name="ids">An array of members ids</param>
        [HttpDelete]
        [Route("members")]
        [Authorize(ModuleConstants.Security.Permissions.Delete)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> DeleteMembers([FromQuery] string[] ids)
        {
            await _memberService.DeleteAsync(ids);
            return NoContent();
        }

        /// <summary>
        /// Bulk delete members
        /// </summary>
        /// <remarks>Bulk delete members by search criteria of members.</remarks>
        /// <param name="criteria">concrete instance of SearchCriteria type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        [HttpPost]
        [Route("members/delete")]
        [Authorize(ModuleConstants.Security.Permissions.Delete)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> BulkDeleteMembersBySearchCriteria([FromBody] MembersSearchCriteria criteria)
        {
            bool hasSearchCriteriaMembers;
            var idsToDelete = new List<string>();
            do
            {
                var searchResult = await _memberSearchService.SearchMembersAsync(criteria);
                hasSearchCriteriaMembers = searchResult.Results.Any();
                if (hasSearchCriteriaMembers)
                {
                    foreach (var member in searchResult.Results)
                    {
                        idsToDelete.Add(member.Id);
                    }

                    criteria.Skip += criteria.Take;
                }
            }
            while (hasSearchCriteriaMembers);

            for (var i = 0; i < idsToDelete.Count; i += criteria.Take)
            {
                await _memberService.DeleteAsync(idsToDelete.Skip(i).Take(criteria.Take).ToArray());
            }

            return NoContent();
        }

        /// <summary>
        /// Partial update for Member
        /// </summary>
        /// <param name="id">Member id</param>
        /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("members/{id}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PatchMember(string id, [FromBody] JsonPatchDocument<Member> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var member = await _memberService.GetByIdAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            if (!(await AuthorizeAsync(member, ModuleConstants.Security.Permissions.Update)).Succeeded)
            {
                return Forbid();
            }

            patchDocument.ApplyTo(member, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (HasDefaultBillingAndShippingAddress(member, out var actionResult))
            {
                return actionResult;
            }

            await _memberService.SaveChangesAsync([member]);

            return NoContent();
        }

        [HttpPost]
        [Route("members/customers/invite")]
        public async Task<ActionResult<InviteCustomerResult>> InviteCustomers([FromBody] InviteCustomerRequest request)
        {
            var result = await _inviteCustomerService.InviteCustomerAsyc(request);
            if (result.Succeeded)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        #region Special members for storefront C# API client  (because it not support polymorph types)

        #region Contact

        /// <summary>
        /// Create contact
        /// </summary>
        [HttpPost]
        [Route("contacts")]
        public async Task<ActionResult<Contact>> CreateContact([FromBody] Contact contact)
        {
            if (!(await AuthorizeAsync(contact, ModuleConstants.Security.Permissions.Create)).Succeeded)
            {
                return Forbid();
            }
            await _memberService.SaveChangesAsync([contact]);
            return Ok(contact);
        }

        /// <summary>
        /// Bulk create contacts
        /// </summary>
        [HttpPost]
        [Route("contacts/bulk")]
        public async Task<ActionResult<Contact[]>> BulkCreateContacts([FromBody] Contact[] contacts)
        {
            if (!(await AuthorizeAsync(contacts, ModuleConstants.Security.Permissions.Create)).Succeeded)
            {
                return Forbid();
            }
            await _memberService.SaveChangesAsync(contacts);
            return Ok(contacts);
        }

        /// <summary>
        /// Update contact
        /// </summary>
        [HttpPut]
        [Route("contacts")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult<Contact>> UpdateContact([FromBody] Contact contact)
        {
            if (!(await AuthorizeAsync(contact, ModuleConstants.Security.Permissions.Update)).Succeeded)
            {
                return Forbid();
            }
            await _memberService.SaveChangesAsync([contact]);
            return NoContent();
        }

        /// <summary>
        /// Bulk update contact
        /// </summary>
        [HttpPut]
        [Route("contacts/bulk")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult<Contact[]>> BulkUpdateContacts([FromBody] Contact[] contacts)
        {
            if (!(await AuthorizeAsync(contacts, ModuleConstants.Security.Permissions.Update)).Succeeded)
            {
                return Forbid();
            }
            await _memberService.SaveChangesAsync(contacts);
            return NoContent();
        }

        /// <summary>
        /// Partial update for Contact
        /// </summary>
        /// <param name="id">Contact id</param>
        /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("contacts/{id}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PatchContact(string id, [FromBody] JsonPatchDocument<Contact> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var contact = await _memberService.GetByIdAsync(id, responseGroup: null, memberType: nameof(Contact)) as Contact;
            if (contact == null)
            {
                return NotFound();
            }

            if (!(await AuthorizeAsync(contact, ModuleConstants.Security.Permissions.Update)).Succeeded)
            {
                return Forbid();
            }

            patchDocument.ApplyTo(contact, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _memberService.SaveChangesAsync([contact]);

            return NoContent();
        }

        /// <summary>
        /// Delete contacts
        /// </summary>
        /// <remarks>Delete contacts by given array of ids.</remarks>
        /// <param name="ids">An array of contacts ids</param>
        [HttpDelete]
        [Route("contacts")]
        [Authorize(ModuleConstants.Security.Permissions.Delete)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public Task<ActionResult> DeleteContacts([FromQuery] string[] ids)
        {
            return DeleteMembers(ids);
        }

        /// <summary>
        /// Get contact
        /// </summary>
        /// <param name="id">Contact ID</param>
        [HttpGet]
        [Route("contacts/{id}")]
        public async Task<ActionResult<Contact>> GetContactById(string id)
        {
            var result = await _memberService.GetByIdAsync(id, null, typeof(Contact).Name);
            if (!(await AuthorizeAsync(result, ModuleConstants.Security.Permissions.Read)).Succeeded)
            {
                return Forbid();
            }
            return Ok(result);
        }

        /// <summary>
        /// Get plenty contacts
        /// </summary>
        /// <param name="ids">contact IDs</param>
        [HttpGet]
        [Route("contacts")]
        public async Task<ActionResult<Contact[]>> GetContactsByIds([FromQuery] string[] ids)
        {
            var result = await _memberService.GetByIdsAsync(ids, null, [typeof(Contact).Name]);
            if (!(await AuthorizeAsync(result, ModuleConstants.Security.Permissions.Read)).Succeeded)
            {
                return Forbid();
            }
            return Ok(result);
        }

        /// <summary>
        /// Search contacts
        /// </summary>
        /// <remarks>Get array of contacts satisfied search criteria.</remarks>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        [HttpPost]
        [Route("contacts/search")]
        public async Task<ActionResult<ContactSearchResult>> SearchContacts([FromBody] MembersSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = AbstractTypeFactory<MembersSearchCriteria>.TryCreateInstance();
            }

            criteria.MemberType = typeof(Contact).Name;
            criteria.MemberTypes = [criteria.MemberType];

            if (!(await AuthorizeAsync(criteria, ModuleConstants.Security.Permissions.Read)).Succeeded)
            {
                return Forbid();
            }

            var searchResult = await _memberSearchService.SearchMembersAsync(criteria);

            var result = new ContactSearchResult
            {
                TotalCount = searchResult.TotalCount,
                Results = searchResult.Results.OfType<Contact>().ToList()
            };

            return Ok(result);
        }

        #endregion

        #region Organization

        /// <summary>
        /// Create organization
        /// </summary>
        [HttpPost]
        [Route("organizations")]
        public async Task<ActionResult<Organization>> CreateOrganization([FromBody] Organization organization)
        {
            if (!(await AuthorizeAsync(organization, ModuleConstants.Security.Permissions.Create)).Succeeded)
            {
                return Forbid();
            }
            await _memberService.SaveChangesAsync([organization]);
            return Ok(organization);
        }

        /// <summary>
        /// Bulk create organizations
        /// </summary>
        [HttpPost]
        [Route("organizations/bulk")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult<Organization[]>> BulkCreateOrganizations([FromBody] Organization[] organizations)
        {
            if (!(await AuthorizeAsync(organizations, ModuleConstants.Security.Permissions.Create)).Succeeded)
            {
                return Forbid();
            }

            await _memberService.SaveChangesAsync(organizations);
            return NoContent();
        }

        /// <summary>
        /// Update organization
        /// </summary>
        [HttpPut]
        [Route("organizations")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult<Organization>> UpdateOrganization([FromBody] Organization organization)
        {
            if (!(await AuthorizeAsync(organization, ModuleConstants.Security.Permissions.Update)).Succeeded)
            {
                return Forbid();
            }
            await _memberService.SaveChangesAsync([organization]);
            return NoContent();
        }

        /// <summary>
        /// Bulk update organization
        /// </summary>
        [HttpPut]
        [Route("organizations/bulk")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult<Organization[]>> BulkUpdateOrganizations([FromBody] Organization[] organizations)
        {
            if (!(await AuthorizeAsync(organizations, ModuleConstants.Security.Permissions.Update)).Succeeded)
            {
                return Forbid();
            }
            await _memberService.SaveChangesAsync(organizations);
            return NoContent();
        }

        /// <summary>
        /// Delete organizations
        /// </summary>
        /// <remarks>Delete organizations by given array of ids.</remarks>
        /// <param name="ids">An array of organizations ids</param>
        [HttpDelete]
        [Route("organizations")]
        [Authorize(ModuleConstants.Security.Permissions.Delete)]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public Task<ActionResult> DeleteOrganizations([FromQuery] string[] ids)
        {
            return DeleteMembers(ids);
        }

        /// <summary>
        /// Get organization
        /// </summary>
        /// <param name="id">Organization id</param>
        [HttpGet]
        [Route("organizations/{id}")]
        public async Task<ActionResult<Organization>> GetOrganizationById(string id)
        {
            var result = await _memberService.GetByIdAsync(id, null, typeof(Organization).Name);
            if (!(await AuthorizeAsync(result, ModuleConstants.Security.Permissions.Read)).Succeeded)
            {
                return Forbid();
            }
            return Ok(result);
        }

        /// <summary>
        /// Get plenty organizations
        /// </summary>
        /// <param name="ids">Organization ids</param>
        [HttpGet]
        [Route("organizations")]
        public async Task<ActionResult<Organization[]>> GetOrganizationsByIds([FromQuery] string[] ids)
        {
            var result = await _memberService.GetByIdsAsync(ids, null, [typeof(Organization).Name]);
            if (!(await AuthorizeAsync(result, ModuleConstants.Security.Permissions.Read)).Succeeded)
            {
                return Forbid();
            }
            return Ok(result);
        }

        /// <summary>
        /// Search organizations
        /// </summary>
        /// <remarks>Get array of organizations satisfied search criteria.</remarks>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        [HttpPost]
        [Route("organizations/search")]
        public async Task<ActionResult<OrganizationSearchResult>> SearchOrganizations([FromBody] MembersSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = AbstractTypeFactory<MembersSearchCriteria>.TryCreateInstance();
            }

            criteria.MemberType = typeof(Organization).Name;
            criteria.MemberTypes = [criteria.MemberType];

            if (!(await AuthorizeAsync(criteria, ModuleConstants.Security.Permissions.Read)).Succeeded)
            {
                return Forbid();
            }

            var searchResult = await _memberSearchService.SearchMembersAsync(criteria);

            var result = new OrganizationSearchResult
            {
                TotalCount = searchResult.TotalCount,
                Results = searchResult.Results.OfType<Organization>().ToList()
            };

            return Ok(result);
        }

        /// <summary>
        /// Partial update for Organization
        /// </summary>
        /// <param name="id">Organization id</param>
        /// <param name="patchDocument">JsonPatchDocument object with fields to update</param>
        /// <returns></returns>
        [HttpPatch]
        [Route("organizations/{id}")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<IActionResult> PatchOrganization(string id, [FromBody] JsonPatchDocument<Organization> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var contact = await _memberService.GetByIdAsync(id, responseGroup: null, memberType: nameof(Organization)) as Organization;
            if (contact == null)
            {
                return NotFound();
            }

            if (!(await AuthorizeAsync(contact, ModuleConstants.Security.Permissions.Update)).Succeeded)
            {
                return Forbid();
            }

            patchDocument.ApplyTo(contact, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _memberService.SaveChangesAsync([contact]);

            return NoContent();
        }

        #endregion

        #region Vendor

        /// <summary>
        /// Get vendor
        /// </summary>
        /// <param name="id">Vendor ID</param>
        [HttpGet]
        [Route("vendors/{id}")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<Vendor>> GetVendorById(string id)
        {
            var result = await _memberService.GetByIdAsync(id, null, typeof(Vendor).Name);
            return Ok(result);
        }

        /// <summary>
        /// Get plenty vendors
        /// </summary>
        /// <param name="ids">Vendors IDs</param>
        [HttpGet]
        [Route("vendors")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<Vendor[]>> GetVendorsByIds([FromQuery] string[] ids)
        {
            var result = await _memberService.GetByIdsAsync(ids, null, new[] { typeof(Vendor).Name });
            return Ok(result);
        }

        /// <summary>
        /// Search vendors
        /// </summary>
        /// <remarks>Get array of vendors satisfied search criteria.</remarks>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        [HttpPost]
        [Route("vendors/search")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<VendorSearchResult>> SearchVendors([FromBody] MembersSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new MembersSearchCriteria();
            }

            criteria.MemberType = typeof(Vendor).Name;
            criteria.MemberTypes = new[] { criteria.MemberType };
            var searchResult = await _memberSearchService.SearchMembersAsync(criteria);

            var result = AbstractTypeFactory<VendorSearchResult>.TryCreateInstance();
            result.TotalCount = searchResult.TotalCount;
            result.Results = searchResult.Results.OfType<Vendor>().ToList();

            return Ok(result);
        }

        #endregion

        [HttpPut]
        [Route("addresses")]
        [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
        public async Task<ActionResult> UpdateAddesses([FromQuery] string memberId, [FromBody] IEnumerable<Address> addresses)
        {
            var member = await _memberService.GetByIdAsync(memberId);
            if (member != null)
            {
                if (!(await AuthorizeAsync(member, ModuleConstants.Security.Permissions.Update)).Succeeded)
                {
                    return Forbid();
                }
                member.Addresses = addresses.ToList();
                await _memberService.SaveChangesAsync([member]);
            }
            return NoContent();
        }

        /// <summary>
        /// Create employee
        /// </summary>
        [HttpPost]
        [Route("employees")]
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public async Task<ActionResult<Employee>> CreateEmployee([FromBody] Employee employee)
        {
            await _memberService.SaveChangesAsync([employee]);
            return Ok(employee);
        }

        /// <summary>
        /// Create employee
        /// </summary>
        [HttpPost]
        [Route("employees/bulk")]
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public async Task<ActionResult<Employee[]>> BulkCreateEmployees([FromBody] Employee[] employees)
        {
            await _memberService.SaveChangesAsync(employees);
            return Ok(employees);
        }

        /// <summary>
        /// Get plenty employees
        /// </summary>
        /// <param name="ids">contact IDs</param>
        [HttpGet]
        [Route("employees")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<Employee[]>> GetEmployeesByIds([FromQuery] string[] ids)
        {
            var result = await _memberService.GetByIdsAsync(ids, null, new[] { typeof(Employee).Name });
            return Ok(result);
        }

        /// <summary>
        /// Get all member organizations
        /// </summary>
        /// <param name="id">member Id</param>
        [HttpGet]
        [Route("members/{id}/organizations")]
        public async Task<ActionResult<Organization[]>> GetMemberOrganizations(string id)
        {
            var members = await _memberService.GetByIdsAsync([id], null, [typeof(Employee).Name, typeof(Contact).Name]);
            var member = members.FirstOrDefault();
            var organizationsIds = new List<string>();
            if (member != null)
            {
                if (member is Contact contact)
                {
                    organizationsIds = contact.Organizations?.ToList() ?? organizationsIds;
                }
                else if (member is Employee employee)
                {
                    organizationsIds = employee.Organizations?.ToList() ?? organizationsIds;
                }
            }

            return await GetOrganizationsByIds([.. organizationsIds]);
        }

        #endregion

        private bool HasDefaultBillingAndShippingAddress(Member member, out ActionResult actionResult)
        {
            actionResult = null;
            var hasDefaultBillingAndShippingAddress = member?.Addresses?.Any(a => a.IsDefault && a.AddressType == AddressType.BillingAndShipping) ?? false;

            if (hasDefaultBillingAndShippingAddress)
            {
                actionResult = BadRequest("BillingAndShipping address cannot be set as default");
            }

            return hasDefaultBillingAndShippingAddress;
        }

        private Task<AuthorizationResult> AuthorizeAsync(object resource, string permission)
        {
            return _authorizationService.AuthorizeAsync(User, resource, new CustomerAuthorizationRequirement(permission));
        }
    }
}
