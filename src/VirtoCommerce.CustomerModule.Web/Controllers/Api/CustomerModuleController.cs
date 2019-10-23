using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.CustomerModule.Core;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Web.Controllers.Api
{
    [Route("api")]

    public class CustomerModuleController : Controller
    {
        private readonly IMemberService _memberService;
        private readonly IMemberSearchService _memberSearchService;

        public CustomerModuleController(IMemberService memberService, IMemberSearchService memberSearchService)
        {
            _memberService = memberService;
            _memberSearchService = memberSearchService;
        }

        /// <summary>
        /// Get organizations
        /// </summary>
        /// <remarks>Get array of all organizations.</remarks>
        [HttpGet]
        [Route("members/organizations")]
        [Authorize(ModuleConstants.Security.Permissions.Access)]
        public async Task<ActionResult<Organization[]>> ListOrganizations()
        {
            var searchCriteria = new MembersSearchCriteria
            {
                MemberType = typeof(Organization).Name,
                DeepSearch = true,
                Take = int.MaxValue
            };
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
        [Authorize(ModuleConstants.Security.Permissions.Access)]
        public async Task<ActionResult<MemberSearchResult>> SearchMember([FromBody] MembersSearchCriteria criteria)
        {
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
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<Member>> GetMemberById(string id, [FromQuery] string responseGroup = null, [FromQuery]  string memberType = null)
        {
            //pass member type name for better perfomance
            var retVal = await _memberService.GetByIdAsync(id, responseGroup, memberType);
            if (retVal != null)
            {
                // Casting to dynamic fixes a serialization error in XML formatter when the returned object type is derived from the Member class.
                return Ok((dynamic)retVal);
            }
            return Ok();
        }

        [HttpGet]
        [Route("members")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<Member[]>> GetMembersByIds([FromQuery] string[] ids, [FromQuery]  string responseGroup = null, [FromQuery]  string[] memberTypes = null)
        {
            //pass member types name for better performance
            var retVal = await _memberService.GetByIdsAsync(ids, responseGroup, memberTypes);
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
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public async Task<ActionResult<Member>> CreateMember([FromBody] Member member)
        {
            await _memberService.SaveChangesAsync(new[] { member });
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
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public async Task<ActionResult<Member[]>> BulkCreateMembers([FromBody] Member[] members)
        {
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
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public async Task<ActionResult> UpdateMember([FromBody] Member member)
        {
            await _memberService.SaveChangesAsync(new[] { member });
            return NoContent();
        }

        /// <summary>
        /// Bulk update members
        /// </summary>
        /// <param name="members">Array of concrete instances of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        [HttpPut]
        [Route("members/bulk")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public async Task<ActionResult> BulkUpdateMembers([FromBody] Member[] members)
        {
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
        public async Task<ActionResult> BulkDeleteMembersBySearchCriteria([FromBody] MembersSearchCriteria criteria)
        {
            bool hasSearchCriteriaMembers;
            var listIds = new List<string>();
            do
            {
                var searchResult = await _memberSearchService.SearchMembersAsync(criteria);
                hasSearchCriteriaMembers = searchResult.Results.Any();
                if (hasSearchCriteriaMembers)
                {
                    foreach (var member in searchResult.Results)
                    {
                        listIds.Add(member.Id);
                    }

                    criteria.Skip += criteria.Take;
                }
            }
            while (hasSearchCriteriaMembers);

            listIds.ProcessWithPaging(criteria.Take, async (ids, currentItem, totalCount) =>
            {
                await _memberService.DeleteAsync(ids.ToArray());
            });


            return NoContent();
        }

        #region Special members for storefront C# API client  (because it not support polymorph types)

        /// <summary>
        /// Create contact
        /// </summary>
        [HttpPost]
        [Route("contacts")]
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public Task<ActionResult<Contact>> CreateContact([FromBody] Contact contact)
        {
            return UpdateContact(contact);
        }

        /// <summary>
        /// Bulk create contacts
        /// </summary>
        [HttpPost]
        [Route("contacts/bulk")]
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public Task<ActionResult<Contact[]>> BulkCreateContacts([FromBody] Contact[] contacts)
        {
            return BulkUpdateContacts(contacts);
        }
    

        /// <summary>
        /// Update contact
        /// </summary>
        [HttpPut]
        [Route("contacts")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public async Task<ActionResult<Contact>> UpdateContact([FromBody] Contact contact)
        {
            await _memberService.SaveChangesAsync(new[] { contact });
            return Ok(contact);
        }

        /// <summary>
        /// Bulk update contact
        /// </summary>
        [HttpPut]
        [Route("contacts/bulk")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public async Task<ActionResult<Contact[]>> BulkUpdateContacts([FromBody] Contact[] contacts)
        {
            await _memberService.SaveChangesAsync(contacts);
            return Ok(contacts);
        }

        /// <summary>
        /// Create organization
        /// </summary>
        [HttpPost]
        [Route("organizations")]
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public Task<ActionResult<Organization>> CreateOrganization([FromBody] Organization organization)
        {
            return UpdateOrganization(organization);
        }

        /// <summary>
        /// Bulk create organizations
        /// </summary>
        [HttpPost]
        [Route("organizations/bulk")]
        [Authorize(ModuleConstants.Security.Permissions.Create)]
        public Task<ActionResult<Organization[]>> BulkCreateOrganizations([FromBody] Organization[] organizations)
        {
            return BulkUpdateOrganizations(organizations);
        }

        /// <summary>
        /// Update organization
        /// </summary>
        [HttpPut]
        [Route("organizations")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public async Task<ActionResult<Organization>> UpdateOrganization([FromBody]Organization organization)
        {
            await _memberService.SaveChangesAsync(new[] { organization });
            return Ok(organization);
        }

        /// <summary>
        /// Bulk update organization
        /// </summary>
        [HttpPut]
        [Route("organizations/bulk")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public async Task<ActionResult<Organization[]>> BulkUpdateOrganizations([FromBody] Organization[] organizations)
        {
            await _memberService.SaveChangesAsync(organizations);
            return Ok(organizations);
        }

        /// <summary>
        /// Delete organizations
        /// </summary>
        /// <remarks>Delete organizations by given array of ids.</remarks>
        /// <param name="ids">An array of organizations ids</param>
        [HttpDelete]
        [Route("organizations")]
        [Authorize(ModuleConstants.Security.Permissions.Delete)]
        public Task<ActionResult> DeleteOrganizations([FromQuery] string[] ids)
        {
            return DeleteMembers(ids);
        }

        /// <summary>
        /// Delete contacts
        /// </summary>
        /// <remarks>Delete contacts by given array of ids.</remarks>
        /// <param name="ids">An array of contacts ids</param>
        [HttpDelete]
        [Route("contacts")]
        [Authorize(ModuleConstants.Security.Permissions.Delete)]
        public Task<ActionResult> DeleteContacts([FromQuery] string[] ids)
        {
            return DeleteMembers(ids);
        }

        /// <summary>
        /// Get organization
        /// </summary>
        /// <param name="id">Organization id</param>
        [HttpGet]
        [Route("organizations/{id}")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<Organization>> GetOrganizationById(string id)
        {
            var result = await _memberService.GetByIdAsync(id, null, typeof(Organization).Name);
            return Ok(result);
        }

        /// <summary>
        /// Get plenty organizations 
        /// </summary>
        /// <param name="ids">Organization ids</param>
        [HttpGet]
        [Route("organizations")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<Organization[]>> GetOrganizationsByIds([FromQuery] string[] ids)
        {
            var result = await _memberService.GetByIdsAsync(ids, null, new[] { typeof(Organization).Name });
            return Ok(result);
        }

        /// <summary>
        /// Search organizations
        /// </summary>
        /// <remarks>Get array of organizations satisfied search criteria.</remarks>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        [HttpPost]
        [Route("organizations/search")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<OrganizationSearchResult>> SearchOrganizations([FromBody] MembersSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = AbstractTypeFactory<MembersSearchCriteria>.TryCreateInstance();
            }

            criteria.MemberType = typeof(Organization).Name;
            criteria.MemberTypes = new[] { criteria.MemberType };
            var searchResult = await _memberSearchService.SearchMembersAsync(criteria);

            var result = new OrganizationSearchResult
            {
                TotalCount = searchResult.TotalCount,
                Results = searchResult.Results.OfType<Organization>().ToList()
            };

            return Ok(result);
        }

        /// <summary>
        /// Get contact
        /// </summary>
        /// <param name="id">Contact ID</param>
        [HttpGet]
        [Route("contacts/{id}")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<Contact>> GetContactById(string id)
        {
            var result = await _memberService.GetByIdAsync(id, null, typeof(Contact).Name);
            return Ok(result);
        }


        /// <summary>
        /// Get plenty contacts 
        /// </summary>
        /// <param name="ids">contact IDs</param>
        [HttpGet]
        [Route("contacts")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<Contact[]>> GetContactsByIds([FromQuery]string[] ids)
        {
            var result = await _memberService.GetByIdsAsync(ids, null, new[] { typeof(Contact).Name });
            return Ok(result);
        }

        /// <summary>
        /// Search contacts
        /// </summary>
        /// <remarks>Get array of contacts satisfied search criteria.</remarks>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        [HttpPost]
        [Route("contacts/search")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<ContactSearchResult>> SearchContacts(MembersSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = AbstractTypeFactory<MembersSearchCriteria>.TryCreateInstance();
            }

            criteria.MemberType = typeof(Contact).Name;
            criteria.MemberTypes = new[] { criteria.MemberType };
            var searchResult = await _memberSearchService.SearchMembersAsync(criteria);

            var result = new ContactSearchResult
            {
                TotalCount = searchResult.TotalCount,
                Results = searchResult.Results.OfType<Contact>().ToList()
            };

            return Ok(result);
        }

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
        public async Task<ActionResult<Vendor[]>> GetVendorsByIds([FromQuery]string[] ids)
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
        public async Task<ActionResult<VendorSearchResult>> SearchVendors([FromBody]MembersSearchCriteria criteria)
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

        [HttpPut]
        [Route("addresses")]
        [Authorize(ModuleConstants.Security.Permissions.Update)]
        public async Task<ActionResult> UpdateAddesses([FromQuery] string memberId, [FromBody] IEnumerable<Address> addresses)
        {
            var member = await _memberService.GetByIdAsync(memberId);
            if (member != null)
            {
                member.Addresses = addresses.ToList();
                await _memberService.SaveChangesAsync(new[] { member });
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
            await _memberService.SaveChangesAsync(new[] { employee });
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
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public async Task<ActionResult<Organization[]>> GetMemberOrganizations([FromQuery] string id)
        {
            var members = await _memberService.GetByIdsAsync(new[] { id }, null, new[] { typeof(Employee).Name, typeof(Contact).Name });
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
            return await GetOrganizationsByIds(organizationsIds.ToArray());
        }
        #endregion
    }
}
