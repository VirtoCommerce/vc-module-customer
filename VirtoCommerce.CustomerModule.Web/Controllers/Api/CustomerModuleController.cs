using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Practices.ObjectBuilder2;
using VirtoCommerce.CustomerModule.Web.Model;
using VirtoCommerce.CustomerModule.Web.Security;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Web.Security;

namespace VirtoCommerce.CustomerModule.Web.Controllers.Api
{
    [RoutePrefix("api")]
    [CheckPermission(Permission = CustomerPredefinedPermissions.Read)]
    public class CustomerModuleController : ApiController
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
        [ResponseType(typeof(Organization[]))]
        public IHttpActionResult ListOrganizations()
        {
            var searchCriteria = new MembersSearchCriteria
            {
                MemberType = typeof(Organization).Name,
                DeepSearch = true,
                Take = int.MaxValue
            };
            var result = _memberSearchService.SearchMembers(searchCriteria);

            return Ok(result.Results.OfType<Organization>());
        }

        /// <summary>
        /// Get members
        /// </summary>
        /// <remarks>Get array of members satisfied search criteria.</remarks>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        [HttpPost]
        [Route("members/search")]
        [ResponseType(typeof(GenericSearchResult<Member>))]
        public IHttpActionResult Search(MembersSearchCriteria criteria)
        {
            var result = _memberSearchService.SearchMembers(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Get member
        /// </summary>
        /// <param name="id">member id</param>
        /// <param name="responseGroup">Response group flags controls fullness of resulting object graph</param>
        /// <param name="memberType">member type</param>
        [HttpGet]
        [Route("members/{id}")]
        [ResponseType(typeof(Member))]
        public IHttpActionResult GetMemberById(string id, [FromUri]string responseGroup = null, string memberType = null)
        {
            //pass member type name for better perfomance
            var retVal = _memberService.GetByIds(new[] { id }, responseGroup, memberType != null ? new[] { memberType } : null).FirstOrDefault();
            if (retVal != null)
            {
                // Casting to dynamic fixes a serialization error in XML formatter when the returned object type is derived from the Member class.
                return Ok((dynamic)retVal);
            }
            return Ok();
        }

        [HttpGet]
        [Route("members")]
        [ResponseType(typeof(Member[]))]
        public IHttpActionResult GetMembersByIds([FromUri] string[] ids, [FromUri]string responseGroup = null, string[] memberTypes = null)
        {
            //pass member types name for better perfomance
            var retVal = _memberService.GetByIds(ids, responseGroup, memberTypes != null ? memberTypes : null);
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
        [ResponseType(typeof(Member))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Create)]
        public IHttpActionResult CreateMember([FromBody] Member member)
        {
            _memberService.SaveChanges(new[] { member });
            var retVal = _memberService.GetByIds(new[] { member.Id }, null, new[] { member.MemberType }).FirstOrDefault();

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
        [ResponseType(typeof(Member[]))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Create)]
        public IHttpActionResult BulkCreateMembers([FromBody] Member[] members)
        {
            _memberService.SaveChanges(members);
            var retVal = _memberService.GetByIds(members.Select(m => m.Id).ToArray(), null, members.Select(m => m.MemberType).ToArray()).ToArray();

            // Casting to dynamic fixes a serialization error in XML formatter when the returned object type is derived from the Member class.
            return Ok((dynamic)retVal);
        }

        /// <summary>
        /// Update member
        /// </summary>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        [HttpPut]
        [Route("members")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Update)]
        public IHttpActionResult UpdateMember(Member member)
        {
            _memberService.SaveChanges(new[] { member });
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Bulk update members
        /// </summary>
        /// <param name="members">Array of concrete instances of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        [HttpPut]
        [Route("members/bulk")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Update)]
        public IHttpActionResult BulkUpdateMembers(Member[] members)
        {
            _memberService.SaveChanges(members);
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Delete members
        /// </summary>
        /// <remarks>Delete members by given array of ids.</remarks>
        /// <param name="ids">An array of members ids</param>
        [HttpDelete]
        [Route("members")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Delete)]
        public IHttpActionResult DeleteMembers([FromUri] string[] ids)
        {
            _memberService.Delete(ids);
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Bulk delete members
        /// </summary>
        /// <remarks>Bulk delete members by search criteria of members.</remarks>
        /// <param name="criteria">concrete instance of SearchCriteria type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        [HttpPost]
        [Route("members/delete")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Delete)]
        public IHttpActionResult BulkDeleteMembersBySearchCriteria(MembersSearchCriteria criteria)
        {
            bool hasSearchCriteriaMembers;
            var listIds = new List<string>();
            do
            {
                var searchResult = _memberSearchService.SearchMembers(criteria);
                hasSearchCriteriaMembers = searchResult.Results.Any();
                if (hasSearchCriteriaMembers)
                {
                    searchResult.Results.ForEach(res => listIds.Add(res.Id));
                    criteria.Skip += criteria.Take;
                }
            }
            while (hasSearchCriteriaMembers);

            listIds.ProcessWithPaging(criteria.Take, (ids, currentItem, totalCount) =>
            {
                _memberService.Delete(ids.ToArray());
            });


            return StatusCode(HttpStatusCode.NoContent);
        }

        #region Special members for storefront C# API client  (because it not support polymorph types)

        /// <summary>
        /// Create contact
        /// </summary>
        [HttpPost]
        [Route("contacts")]
        [ResponseType(typeof(Contact))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Create)]
        public IHttpActionResult CreateContact(Contact contact)
        {
            return CreateMember(contact);
        }

        /// <summary>
        /// Bulk create contacts
        /// </summary>
        [HttpPost]
        [Route("contacts/bulk")]
        [ResponseType(typeof(Contact[]))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Create)]
        public IHttpActionResult BulkCreateContacts(Contact[] contacts)
        {
            return BulkCreateMembers(contacts.Cast<Member>().ToArray());
        }

        /// <summary>
        /// Update contact
        /// </summary>
        [HttpPut]
        [Route("contacts")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Update)]
        public IHttpActionResult UpdateContact(Contact contact)
        {
            return UpdateMember(contact);
        }

        /// <summary>
        /// Bulk update contact
        /// </summary>
        [HttpPut]
        [Route("contacts/bulk")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Update)]
        public IHttpActionResult BulkUpdateContacts(Contact[] contacts)
        {
            return BulkUpdateMembers(contacts.Cast<Member>().ToArray());
        }

        /// <summary>
        /// Create organization
        /// </summary>
        [HttpPost]
        [Route("organizations")]
        [ResponseType(typeof(Organization))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Create)]
        public IHttpActionResult CreateOrganization(Organization organization)
        {
            return CreateMember(organization);
        }

        /// <summary>
        /// Bulk create organizations
        /// </summary>
        [HttpPost]
        [Route("organizations/bulk")]
        [ResponseType(typeof(Organization[]))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Create)]
        public IHttpActionResult BulkCreateOrganizations(Organization[] organizations)
        {
            return BulkCreateMembers(organizations.Cast<Member>().ToArray());
        }

        /// <summary>
        /// Update organization
        /// </summary>
        [HttpPut]
        [Route("organizations")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Update)]
        public IHttpActionResult UpdateOrganization(Organization organization)
        {
            return UpdateMember(organization);
        }

        /// <summary>
        /// Bulk update organization
        /// </summary>
        [HttpPut]
        [Route("organizations/bulk")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Update)]
        public IHttpActionResult BulkUpdateOrganizations(Organization[] organizations)
        {
            return BulkUpdateMembers(organizations.Cast<Member>().ToArray());
        }

        /// <summary>
        /// Delete organizations
        /// </summary>
        /// <remarks>Delete organizations by given array of ids.</remarks>
        /// <param name="ids">An array of organizations ids</param>
        [HttpDelete]
        [Route("organizations")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Delete)]
        public IHttpActionResult DeleteOrganizations([FromUri] string[] ids)
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
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Delete)]
        public IHttpActionResult DeleteContacts([FromUri] string[] ids)
        {
            return DeleteMembers(ids);
        }

        /// <summary>
        /// Get organization
        /// </summary>
        /// <param name="id">Organization id</param>
        /// <param name="responseGroup">Response group flags controls fullness of resulting object graph</param>
        [HttpGet]
        [Route("organizations/{id}")]
        [ResponseType(typeof(Organization))]
        public IHttpActionResult GetOrganizationById(string id, [FromUri]string responseGroup = null)
        {
            return GetMemberById(id, responseGroup, typeof(Organization).Name);
        }

        /// <summary>
        /// Get plenty organizations
        /// </summary>
        /// <param name="ids">Organization ids</param>
        /// <param name="responseGroup">Response group flags controls fullness of resulting object graph</param>
        [HttpGet]
        [Route("organizations")]
        [ResponseType(typeof(Organization[]))]
        [Obsolete("Backward compatibility. Use GetPlentyOrganizationsByIds instead.")]
        public IHttpActionResult GetOrganizationsByIds([FromUri]string[] ids, [FromUri]string responseGroup = null)
        {
            return GetMembersByIds(ids, responseGroup, new[] { typeof(Organization).Name });
        }

        /// <summary>
        /// Get plenty organizations
        /// </summary>
        /// <param name="ids">Organization ids</param>
        /// <param name="responseGroup">Response group flags controls fullness of resulting object graph</param>
        [HttpPost]
        [Route("organizations/plenty")]
        [ResponseType(typeof(Organization[]))]
        public IHttpActionResult GetPlentyOrganizationsByIds([FromBody]string[] ids, [FromUri]string responseGroup = null)
        {
            return GetMembersByIds(ids, responseGroup, new[] { typeof(Organization).Name });
        }

        /// <summary>
        /// Search organizations
        /// </summary>
        /// <remarks>Get array of organizations satisfied search criteria.</remarks>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        [HttpPost]
        [Route("organizations/search")]
        [ResponseType(typeof(GenericSearchResult<Organization>))]
        public IHttpActionResult SearchOrganizations(MembersSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new MembersSearchCriteria();
            }

            criteria.MemberType = typeof(Organization).Name;
            criteria.MemberTypes = new[] { criteria.MemberType };
            var searchResult = _memberSearchService.SearchMembers(criteria);

            var result = new GenericSearchResult<Organization>
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
        /// <param name="responseGroup">Response group flags controls fullness of resulting object graph
        /// Possible values: Info,WithAncestors, WithNotes, WithEmails, WithAddresses, WithPhones,  WithGroups, WithSecurityAccounts, WithDynamicProperties, WithSeo
        /// Default value: Full
        /// </param>
        [HttpGet]
        [Route("contacts/{id}")]
        [ResponseType(typeof(Contact))]
        public IHttpActionResult GetContactById(string id, [FromUri]string responseGroup = null)
        {
            return GetMemberById(id, responseGroup, typeof(Contact).Name);
        }


        /// <summary>
        /// Get plenty contacts
        /// </summary>
        /// <param name="ids">contact IDs</param>
        /// <param name="responseGroup">Response group flags controls fullness of resulting object graph
        /// Possible values: Info,WithAncestors, WithNotes, WithEmails, WithAddresses, WithPhones,  WithGroups, WithSecurityAccounts, WithDynamicProperties, WithSeo
        /// Default value: Full
        /// </param>
        [HttpGet]
        [Route("contacts")]
        [ResponseType(typeof(Contact[]))]
        public IHttpActionResult GetContactsByIds([FromUri]string[] ids, [FromUri]string responseGroup = null)
        {
            return GetMembersByIds(ids, responseGroup, new[] { typeof(Contact).Name });
        }

        /// <summary>
        /// Search contacts
        /// </summary>
        /// <remarks>Get array of contacts satisfied search criteria.</remarks>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        [HttpPost]
        [Route("contacts/search")]
        [ResponseType(typeof(GenericSearchResult<Contact>))]
        public IHttpActionResult SearchContacts(MembersSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new MembersSearchCriteria();
            }

            criteria.MemberType = typeof(Contact).Name;
            criteria.MemberTypes = new[] { criteria.MemberType };
            var searchResult = _memberSearchService.SearchMembers(criteria);

            var result = new GenericSearchResult<Contact>
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
        /// <param name="responseGroup">Response group flags controls fullness of resulting object graph</param>
        [HttpGet]
        [Route("vendors/{id}")]
        [ResponseType(typeof(Vendor))]
        public IHttpActionResult GetVendorById(string id, [FromUri]string responseGroup = null)
        {
            return GetMemberById(id, responseGroup, typeof(Vendor).Name);
        }

        /// <summary>
        /// Get plenty vendors
        /// </summary>
        /// <param name="ids">Vendors IDs</param>
        /// <param name="responseGroup">Response group flags controls fullness of resulting object graph</param>
        [HttpGet]
        [Route("vendors")]
        [ResponseType(typeof(Vendor[]))]
        public IHttpActionResult GetVendorsByIds([FromUri]string[] ids, [FromUri]string responseGroup = null)
        {
            return GetMembersByIds(ids, responseGroup, new[] { typeof(Vendor).Name });
        }

        /// <summary>
        /// Search vendors
        /// </summary>
        /// <remarks>Get array of vendors satisfied search criteria.</remarks>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        [HttpPost]
        [Route("vendors/search")]
        [ResponseType(typeof(VendorSearchResult))]
        public IHttpActionResult SearchVendors(MembersSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new MembersSearchCriteria();
            }

            criteria.MemberType = typeof(Vendor).Name;
            criteria.MemberTypes = new[] { criteria.MemberType };
            var searchResult = _memberSearchService.SearchMembers(criteria);

            var result = new VendorSearchResult
            {
                TotalCount = searchResult.TotalCount,
                Vendors = searchResult.Results.OfType<Vendor>().ToList()
            };

            return Ok(result);
        }

        [HttpPut]
        [Route("addresses")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Update)]
        public IHttpActionResult UpdateAddesses(string memberId, [FromBody] IEnumerable<Address> addresses)
        {
            var member = _memberService.GetByIds(new[] { memberId }).FirstOrDefault();
            if (member != null)
            {
                member.Addresses = addresses.ToList();
                _memberService.SaveChanges(new[] { member });
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Create employee
        /// </summary>
        [HttpPost]
        [Route("employees")]
        [ResponseType(typeof(Employee))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Create)]
        public IHttpActionResult CreateEmployee(Employee employee)
        {
            return CreateMember(employee);
        }

        /// <summary>
        /// Create employee
        /// </summary>
        [HttpPost]
        [Route("employees/bulk")]
        [ResponseType(typeof(Employee[]))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Create)]
        public IHttpActionResult BulkCreateEmployees(Employee[] employees)
        {
            return BulkCreateMembers(employees.Cast<Member>().ToArray());
        }

        /// <summary>
        /// Get plenty employees
        /// </summary>
        /// <param name="ids">contact IDs</param>
        /// <param name="responseGroup">Response group flags controls fullness of resulting object graph</param>
        [HttpGet]
        [Route("employees")]
        [ResponseType(typeof(Employee[]))]
        public IHttpActionResult GetEmployeesByIds([FromUri]string[] ids, [FromUri]string responseGroup = null)
        {
            return GetMembersByIds(ids, responseGroup, new[] { typeof(Employee).Name });
        }

        /// <summary>
        /// Get all member organizations
        /// </summary>
        /// <param name="id">member Id</param>
        /// <param name="responseGroup">Response group flags controls fullness of resulting object graph</param>
        [HttpGet]
        [Route("members/{id}/organizations")]
        [ResponseType(typeof(Organization[]))]
        public IHttpActionResult GetMemberOrganizations([FromUri] string id, [FromUri]string responseGroup = null)
        {
            var member = _memberService.GetByIds(new[] { id }, responseGroup, new[] { typeof(Employee).Name, typeof(Contact).Name }).FirstOrDefault();
            var organizationsIds = Enumerable.Empty<string>();

            if (member != null)
            {
                if (member is Contact contact)
                {
                    organizationsIds = contact.Organizations ?? organizationsIds;
                }
                else if (member is Employee employee)
                {
                    organizationsIds = employee.Organizations ?? organizationsIds;
                }
            }

            return GetPlentyOrganizationsByIds(organizationsIds.ToArray());
        }

        #endregion Special members for storefront C# API client  (because it not support polymorph types)
    }
}
