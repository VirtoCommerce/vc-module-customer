using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;
using VirtoCommerce.CustomerModule.Web.Model;
using VirtoCommerce.CustomerModule.Web.Security;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Platform.Core.Web.Security;
using coreModel = VirtoCommerce.Domain.Customer.Model;

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
        [ResponseType(typeof(coreModel.Organization[]))]
        public IHttpActionResult ListOrganizations()
        {
            var searchCriteria = new coreModel.MembersSearchCriteria
            {
                MemberType = typeof(coreModel.Organization).Name,
                DeepSearch = true,
                Take = int.MaxValue
            };
            var result = _memberSearchService.SearchMembers(searchCriteria);

            return Ok(result.Results.OfType<coreModel.Organization>());
        }

        /// <summary>
        /// Get members
        /// </summary>
        /// <remarks>Get array of members satisfied search criteria.</remarks>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        [HttpPost]
        [Route("members/search")]
        [ResponseType(typeof(GenericSearchResult<coreModel.Member>))]
        public IHttpActionResult Search(coreModel.MembersSearchCriteria criteria)
        {
            var result = _memberSearchService.SearchMembers(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Get member
        /// </summary>
        /// <param name="id">member id</param>
        [HttpGet]
        [Route("members/{id}")]
        [ResponseType(typeof(coreModel.Member))]
        public IHttpActionResult GetMemberById(string id)
        {
            var retVal = _memberService.GetByIds(new[] { id }).FirstOrDefault();
            if (retVal != null)
            {
                // Casting to dynamic fixes a serialization error in XML formatter when the returned object type is derived from the Member class.
                return Ok((dynamic)retVal);
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
        [ResponseType(typeof(coreModel.Member))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Create)]
        public IHttpActionResult CreateMember([FromBody]coreModel.Member member)
        {
            _memberService.SaveChanges(new[] { member });
            var retVal = _memberService.GetByIds(new[] { member.Id }).FirstOrDefault();

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
        public IHttpActionResult UpdateMember(coreModel.Member member)
        {
            _memberService.SaveChanges(new[] { member });
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

        #region Special members for storefront C# API client  (because it not support polymorph types)

        /// <summary>
        /// Create contact
        /// </summary>
        [HttpPost]
        [Route("contacts")]
        [ResponseType(typeof(coreModel.Contact))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Create)]
        public IHttpActionResult CreateContact(coreModel.Contact contact)
        {
            return CreateMember(contact);
        }

        /// <summary>
        /// Update contact
        /// </summary>
        [HttpPut]
        [Route("contacts")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Update)]
        public IHttpActionResult UpdateContact(coreModel.Contact contact)
        {
            return UpdateMember(contact);
        }

        /// <summary>
        /// Create organization
        /// </summary>
        [HttpPost]
        [Route("organizations")]
        [ResponseType(typeof(coreModel.Organization))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Create)]
        public IHttpActionResult CreateOrganization(coreModel.Organization organization)
        {
            return CreateMember(organization);
        }

        /// <summary>
        /// Update organization
        /// </summary>
        [HttpPut]
        [Route("organizations")]
        [ResponseType(typeof(void))]
        [CheckPermission(Permission = CustomerPredefinedPermissions.Update)]
        public IHttpActionResult UpdateOrganization(coreModel.Organization organization)
        {
            return UpdateMember(organization);
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
        [HttpGet]
        [Route("organizations/{id}")]
        [ResponseType(typeof(coreModel.Organization))]
        public IHttpActionResult GetOrganizationById(string id)
        {
            return GetMemberById(id);
        }

        /// <summary>
        /// Get contact
        /// </summary>
        /// <param name="id">Contact ID</param>
        [HttpGet]
        [Route("contacts/{id}")]
        [ResponseType(typeof(coreModel.Contact))]
        public IHttpActionResult GetContactById(string id)
        {
            return GetMemberById(id);
        }

        /// <summary>
        /// Get vendor
        /// </summary>
        /// <param name="id">Vendor ID</param>
        [HttpGet]
        [Route("vendors/{id}")]
        [ResponseType(typeof(coreModel.Vendor))]
        public IHttpActionResult GetVendorById(string id)
        {
            return GetMemberById(id);
        }

        /// <summary>
        /// Search vendors
        /// </summary>
        /// <remarks>Get array of vendors satisfied search criteria.</remarks>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        [HttpPost]
        [Route("vendors/search")]
        [ResponseType(typeof(VendorSearchResult))]
        public IHttpActionResult SearchVendors(coreModel.MembersSearchCriteria criteria)
        {
            if (criteria == null)
            {
                criteria = new Domain.Customer.Model.MembersSearchCriteria();               
            }
            criteria.MemberType = typeof(coreModel.Vendor).Name;
            var result = _memberSearchService.SearchMembers(criteria);
            var retVal = new VendorSearchResult
            {
                TotalCount = result.TotalCount,
                Vendors = result.Results.OfType<coreModel.Vendor>().ToList()
            };
            return Ok(result);
        }

        #endregion
    }
}
