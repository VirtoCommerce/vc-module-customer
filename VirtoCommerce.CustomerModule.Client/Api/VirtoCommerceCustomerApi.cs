using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RestSharp;
using VirtoCommerce.CustomerModule.Client.Client;
using VirtoCommerce.CustomerModule.Client.Model;

namespace VirtoCommerce.CustomerModule.Client.Api
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IVirtoCommerceCustomerApi : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Create contact
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="contact"></param>
        /// <returns>Contact</returns>
        Contact CustomerModuleCreateContact(Contact contact);

        /// <summary>
        /// Create contact
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="contact"></param>
        /// <returns>ApiResponse of Contact</returns>
        ApiResponse<Contact> CustomerModuleCreateContactWithHttpInfo(Contact contact);
        /// <summary>
        /// Create new member (can be any object inherited from Member type)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        /// <returns>Member</returns>
        Member CustomerModuleCreateMember(Member member);

        /// <summary>
        /// Create new member (can be any object inherited from Member type)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        /// <returns>ApiResponse of Member</returns>
        ApiResponse<Member> CustomerModuleCreateMemberWithHttpInfo(Member member);
        /// <summary>
        /// Create organization
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="organization"></param>
        /// <returns>Organization</returns>
        Organization CustomerModuleCreateOrganization(Organization organization);

        /// <summary>
        /// Create organization
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="organization"></param>
        /// <returns>ApiResponse of Organization</returns>
        ApiResponse<Organization> CustomerModuleCreateOrganizationWithHttpInfo(Organization organization);
        /// <summary>
        /// Delete contacts
        /// </summary>
        /// <remarks>
        /// Delete contacts by given array of ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of contacts ids</param>
        /// <returns></returns>
        void CustomerModuleDeleteContacts(List<string> ids);

        /// <summary>
        /// Delete contacts
        /// </summary>
        /// <remarks>
        /// Delete contacts by given array of ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of contacts ids</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> CustomerModuleDeleteContactsWithHttpInfo(List<string> ids);
        /// <summary>
        /// Delete members
        /// </summary>
        /// <remarks>
        /// Delete members by given array of ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of members ids</param>
        /// <returns></returns>
        void CustomerModuleDeleteMembers(List<string> ids);

        /// <summary>
        /// Delete members
        /// </summary>
        /// <remarks>
        /// Delete members by given array of ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of members ids</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> CustomerModuleDeleteMembersWithHttpInfo(List<string> ids);
        /// <summary>
        /// Delete organizations
        /// </summary>
        /// <remarks>
        /// Delete organizations by given array of ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of organizations ids</param>
        /// <returns></returns>
        void CustomerModuleDeleteOrganizations(List<string> ids);

        /// <summary>
        /// Delete organizations
        /// </summary>
        /// <remarks>
        /// Delete organizations by given array of ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of organizations ids</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> CustomerModuleDeleteOrganizationsWithHttpInfo(List<string> ids);
        /// <summary>
        /// Get contact
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Contact ID</param>
        /// <returns>Contact</returns>
        Contact CustomerModuleGetContactById(string id);

        /// <summary>
        /// Get contact
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Contact ID</param>
        /// <returns>ApiResponse of Contact</returns>
        ApiResponse<Contact> CustomerModuleGetContactByIdWithHttpInfo(string id);
        /// <summary>
        /// Get member
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">member id</param>
        /// <returns>Member</returns>
        Member CustomerModuleGetMemberById(string id);

        /// <summary>
        /// Get member
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">member id</param>
        /// <returns>ApiResponse of Member</returns>
        ApiResponse<Member> CustomerModuleGetMemberByIdWithHttpInfo(string id);
        /// <summary>
        /// Get organization
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Organization id</param>
        /// <returns>Organization</returns>
        Organization CustomerModuleGetOrganizationById(string id);

        /// <summary>
        /// Get organization
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Organization id</param>
        /// <returns>ApiResponse of Organization</returns>
        ApiResponse<Organization> CustomerModuleGetOrganizationByIdWithHttpInfo(string id);
        /// <summary>
        /// Get vendor
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Vendor ID</param>
        /// <returns>Vendor</returns>
        Vendor CustomerModuleGetVendorById(string id);

        /// <summary>
        /// Get vendor
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Vendor ID</param>
        /// <returns>ApiResponse of Vendor</returns>
        ApiResponse<Vendor> CustomerModuleGetVendorByIdWithHttpInfo(string id);
        /// <summary>
        /// Get organizations
        /// </summary>
        /// <remarks>
        /// Get array of all organizations.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;Organization&gt;</returns>
        List<Organization> CustomerModuleListOrganizations();

        /// <summary>
        /// Get organizations
        /// </summary>
        /// <remarks>
        /// Get array of all organizations.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of List&lt;Organization&gt;</returns>
        ApiResponse<List<Organization>> CustomerModuleListOrganizationsWithHttpInfo();
        /// <summary>
        /// Get members
        /// </summary>
        /// <remarks>
        /// Get array of members satisfied search criteria.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        /// <returns>MembersSearchResult</returns>
        MembersSearchResult CustomerModuleSearch(MembersSearchCriteria criteria);

        /// <summary>
        /// Get members
        /// </summary>
        /// <remarks>
        /// Get array of members satisfied search criteria.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        /// <returns>ApiResponse of MembersSearchResult</returns>
        ApiResponse<MembersSearchResult> CustomerModuleSearchWithHttpInfo(MembersSearchCriteria criteria);
        /// <summary>
        /// Update contact
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="contact"></param>
        /// <returns></returns>
        void CustomerModuleUpdateContact(Contact contact);

        /// <summary>
        /// Update contact
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="contact"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> CustomerModuleUpdateContactWithHttpInfo(Contact contact);
        /// <summary>
        /// Update member
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        /// <returns></returns>
        void CustomerModuleUpdateMember(Member member);

        /// <summary>
        /// Update member
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> CustomerModuleUpdateMemberWithHttpInfo(Member member);
        /// <summary>
        /// Update organization
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="organization"></param>
        /// <returns></returns>
        void CustomerModuleUpdateOrganization(Organization organization);

        /// <summary>
        /// Update organization
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="organization"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> CustomerModuleUpdateOrganizationWithHttpInfo(Organization organization);
        #endregion Synchronous Operations
        #region Asynchronous Operations
        /// <summary>
        /// Create contact
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="contact"></param>
        /// <returns>Task of Contact</returns>
        System.Threading.Tasks.Task<Contact> CustomerModuleCreateContactAsync(Contact contact);

        /// <summary>
        /// Create contact
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="contact"></param>
        /// <returns>Task of ApiResponse (Contact)</returns>
        System.Threading.Tasks.Task<ApiResponse<Contact>> CustomerModuleCreateContactAsyncWithHttpInfo(Contact contact);
        /// <summary>
        /// Create new member (can be any object inherited from Member type)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        /// <returns>Task of Member</returns>
        System.Threading.Tasks.Task<Member> CustomerModuleCreateMemberAsync(Member member);

        /// <summary>
        /// Create new member (can be any object inherited from Member type)
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        /// <returns>Task of ApiResponse (Member)</returns>
        System.Threading.Tasks.Task<ApiResponse<Member>> CustomerModuleCreateMemberAsyncWithHttpInfo(Member member);
        /// <summary>
        /// Create organization
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="organization"></param>
        /// <returns>Task of Organization</returns>
        System.Threading.Tasks.Task<Organization> CustomerModuleCreateOrganizationAsync(Organization organization);

        /// <summary>
        /// Create organization
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="organization"></param>
        /// <returns>Task of ApiResponse (Organization)</returns>
        System.Threading.Tasks.Task<ApiResponse<Organization>> CustomerModuleCreateOrganizationAsyncWithHttpInfo(Organization organization);
        /// <summary>
        /// Delete contacts
        /// </summary>
        /// <remarks>
        /// Delete contacts by given array of ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of contacts ids</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task CustomerModuleDeleteContactsAsync(List<string> ids);

        /// <summary>
        /// Delete contacts
        /// </summary>
        /// <remarks>
        /// Delete contacts by given array of ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of contacts ids</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> CustomerModuleDeleteContactsAsyncWithHttpInfo(List<string> ids);
        /// <summary>
        /// Delete members
        /// </summary>
        /// <remarks>
        /// Delete members by given array of ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of members ids</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task CustomerModuleDeleteMembersAsync(List<string> ids);

        /// <summary>
        /// Delete members
        /// </summary>
        /// <remarks>
        /// Delete members by given array of ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of members ids</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> CustomerModuleDeleteMembersAsyncWithHttpInfo(List<string> ids);
        /// <summary>
        /// Delete organizations
        /// </summary>
        /// <remarks>
        /// Delete organizations by given array of ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of organizations ids</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task CustomerModuleDeleteOrganizationsAsync(List<string> ids);

        /// <summary>
        /// Delete organizations
        /// </summary>
        /// <remarks>
        /// Delete organizations by given array of ids.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of organizations ids</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> CustomerModuleDeleteOrganizationsAsyncWithHttpInfo(List<string> ids);
        /// <summary>
        /// Get contact
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Contact ID</param>
        /// <returns>Task of Contact</returns>
        System.Threading.Tasks.Task<Contact> CustomerModuleGetContactByIdAsync(string id);

        /// <summary>
        /// Get contact
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Contact ID</param>
        /// <returns>Task of ApiResponse (Contact)</returns>
        System.Threading.Tasks.Task<ApiResponse<Contact>> CustomerModuleGetContactByIdAsyncWithHttpInfo(string id);
        /// <summary>
        /// Get member
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">member id</param>
        /// <returns>Task of Member</returns>
        System.Threading.Tasks.Task<Member> CustomerModuleGetMemberByIdAsync(string id);

        /// <summary>
        /// Get member
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">member id</param>
        /// <returns>Task of ApiResponse (Member)</returns>
        System.Threading.Tasks.Task<ApiResponse<Member>> CustomerModuleGetMemberByIdAsyncWithHttpInfo(string id);
        /// <summary>
        /// Get organization
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Organization id</param>
        /// <returns>Task of Organization</returns>
        System.Threading.Tasks.Task<Organization> CustomerModuleGetOrganizationByIdAsync(string id);

        /// <summary>
        /// Get organization
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Organization id</param>
        /// <returns>Task of ApiResponse (Organization)</returns>
        System.Threading.Tasks.Task<ApiResponse<Organization>> CustomerModuleGetOrganizationByIdAsyncWithHttpInfo(string id);
        /// <summary>
        /// Get vendor
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Vendor ID</param>
        /// <returns>Task of Vendor</returns>
        System.Threading.Tasks.Task<Vendor> CustomerModuleGetVendorByIdAsync(string id);

        /// <summary>
        /// Get vendor
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Vendor ID</param>
        /// <returns>Task of ApiResponse (Vendor)</returns>
        System.Threading.Tasks.Task<ApiResponse<Vendor>> CustomerModuleGetVendorByIdAsyncWithHttpInfo(string id);
        /// <summary>
        /// Get organizations
        /// </summary>
        /// <remarks>
        /// Get array of all organizations.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of List&lt;Organization&gt;</returns>
        System.Threading.Tasks.Task<List<Organization>> CustomerModuleListOrganizationsAsync();

        /// <summary>
        /// Get organizations
        /// </summary>
        /// <remarks>
        /// Get array of all organizations.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of ApiResponse (List&lt;Organization&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<Organization>>> CustomerModuleListOrganizationsAsyncWithHttpInfo();
        /// <summary>
        /// Get members
        /// </summary>
        /// <remarks>
        /// Get array of members satisfied search criteria.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        /// <returns>Task of MembersSearchResult</returns>
        System.Threading.Tasks.Task<MembersSearchResult> CustomerModuleSearchAsync(MembersSearchCriteria criteria);

        /// <summary>
        /// Get members
        /// </summary>
        /// <remarks>
        /// Get array of members satisfied search criteria.
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        /// <returns>Task of ApiResponse (MembersSearchResult)</returns>
        System.Threading.Tasks.Task<ApiResponse<MembersSearchResult>> CustomerModuleSearchAsyncWithHttpInfo(MembersSearchCriteria criteria);
        /// <summary>
        /// Update contact
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="contact"></param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task CustomerModuleUpdateContactAsync(Contact contact);

        /// <summary>
        /// Update contact
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="contact"></param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> CustomerModuleUpdateContactAsyncWithHttpInfo(Contact contact);
        /// <summary>
        /// Update member
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task CustomerModuleUpdateMemberAsync(Member member);

        /// <summary>
        /// Update member
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> CustomerModuleUpdateMemberAsyncWithHttpInfo(Member member);
        /// <summary>
        /// Update organization
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="organization"></param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task CustomerModuleUpdateOrganizationAsync(Organization organization);

        /// <summary>
        /// Update organization
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="organization"></param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> CustomerModuleUpdateOrganizationAsyncWithHttpInfo(Organization organization);
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public class VirtoCommerceCustomerApi : IVirtoCommerceCustomerApi
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtoCommerceCustomerApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="apiClient">An instance of ApiClient.</param>
        /// <returns></returns>
        public VirtoCommerceCustomerApi(ApiClient apiClient)
        {
            ApiClient = apiClient;
            Configuration = apiClient.Configuration;
        }

        /// <summary>
        /// Gets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        public string GetBasePath()
        {
            return ApiClient.RestClient.BaseUrl.ToString();
        }

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public Configuration Configuration { get; set; }

        /// <summary>
        /// Gets or sets the API client object
        /// </summary>
        /// <value>An instance of the ApiClient</value>
        public ApiClient ApiClient { get; set; }

        /// <summary>
        /// Create contact 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="contact"></param>
        /// <returns>Contact</returns>
        public Contact CustomerModuleCreateContact(Contact contact)
        {
             ApiResponse<Contact> localVarResponse = CustomerModuleCreateContactWithHttpInfo(contact);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Create contact 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="contact"></param>
        /// <returns>ApiResponse of Contact</returns>
        public ApiResponse<Contact> CustomerModuleCreateContactWithHttpInfo(Contact contact)
        {
            // verify the required parameter 'contact' is set
            if (contact == null)
                throw new ApiException(400, "Missing required parameter 'contact' when calling VirtoCommerceCustomerApi->CustomerModuleCreateContact");

            var localVarPath = "/api/contacts";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (contact.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(contact); // http body (model) parameter
            }
            else
            {
                localVarPostBody = contact; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleCreateContact: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleCreateContact: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Contact>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Contact)ApiClient.Deserialize(localVarResponse, typeof(Contact)));
            
        }

        /// <summary>
        /// Create contact 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="contact"></param>
        /// <returns>Task of Contact</returns>
        public async System.Threading.Tasks.Task<Contact> CustomerModuleCreateContactAsync(Contact contact)
        {
             ApiResponse<Contact> localVarResponse = await CustomerModuleCreateContactAsyncWithHttpInfo(contact);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Create contact 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="contact"></param>
        /// <returns>Task of ApiResponse (Contact)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Contact>> CustomerModuleCreateContactAsyncWithHttpInfo(Contact contact)
        {
            // verify the required parameter 'contact' is set
            if (contact == null)
                throw new ApiException(400, "Missing required parameter 'contact' when calling VirtoCommerceCustomerApi->CustomerModuleCreateContact");

            var localVarPath = "/api/contacts";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (contact.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(contact); // http body (model) parameter
            }
            else
            {
                localVarPostBody = contact; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleCreateContact: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleCreateContact: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Contact>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Contact)ApiClient.Deserialize(localVarResponse, typeof(Contact)));
            
        }
        /// <summary>
        /// Create new member (can be any object inherited from Member type) 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        /// <returns>Member</returns>
        public Member CustomerModuleCreateMember(Member member)
        {
             ApiResponse<Member> localVarResponse = CustomerModuleCreateMemberWithHttpInfo(member);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Create new member (can be any object inherited from Member type) 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        /// <returns>ApiResponse of Member</returns>
        public ApiResponse<Member> CustomerModuleCreateMemberWithHttpInfo(Member member)
        {
            // verify the required parameter 'member' is set
            if (member == null)
                throw new ApiException(400, "Missing required parameter 'member' when calling VirtoCommerceCustomerApi->CustomerModuleCreateMember");

            var localVarPath = "/api/members";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (member.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(member); // http body (model) parameter
            }
            else
            {
                localVarPostBody = member; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleCreateMember: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleCreateMember: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Member>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Member)ApiClient.Deserialize(localVarResponse, typeof(Member)));
            
        }

        /// <summary>
        /// Create new member (can be any object inherited from Member type) 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        /// <returns>Task of Member</returns>
        public async System.Threading.Tasks.Task<Member> CustomerModuleCreateMemberAsync(Member member)
        {
             ApiResponse<Member> localVarResponse = await CustomerModuleCreateMemberAsyncWithHttpInfo(member);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Create new member (can be any object inherited from Member type) 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        /// <returns>Task of ApiResponse (Member)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Member>> CustomerModuleCreateMemberAsyncWithHttpInfo(Member member)
        {
            // verify the required parameter 'member' is set
            if (member == null)
                throw new ApiException(400, "Missing required parameter 'member' when calling VirtoCommerceCustomerApi->CustomerModuleCreateMember");

            var localVarPath = "/api/members";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (member.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(member); // http body (model) parameter
            }
            else
            {
                localVarPostBody = member; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleCreateMember: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleCreateMember: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Member>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Member)ApiClient.Deserialize(localVarResponse, typeof(Member)));
            
        }
        /// <summary>
        /// Create organization 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="organization"></param>
        /// <returns>Organization</returns>
        public Organization CustomerModuleCreateOrganization(Organization organization)
        {
             ApiResponse<Organization> localVarResponse = CustomerModuleCreateOrganizationWithHttpInfo(organization);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Create organization 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="organization"></param>
        /// <returns>ApiResponse of Organization</returns>
        public ApiResponse<Organization> CustomerModuleCreateOrganizationWithHttpInfo(Organization organization)
        {
            // verify the required parameter 'organization' is set
            if (organization == null)
                throw new ApiException(400, "Missing required parameter 'organization' when calling VirtoCommerceCustomerApi->CustomerModuleCreateOrganization");

            var localVarPath = "/api/organizations";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (organization.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(organization); // http body (model) parameter
            }
            else
            {
                localVarPostBody = organization; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleCreateOrganization: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleCreateOrganization: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Organization>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Organization)ApiClient.Deserialize(localVarResponse, typeof(Organization)));
            
        }

        /// <summary>
        /// Create organization 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="organization"></param>
        /// <returns>Task of Organization</returns>
        public async System.Threading.Tasks.Task<Organization> CustomerModuleCreateOrganizationAsync(Organization organization)
        {
             ApiResponse<Organization> localVarResponse = await CustomerModuleCreateOrganizationAsyncWithHttpInfo(organization);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Create organization 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="organization"></param>
        /// <returns>Task of ApiResponse (Organization)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Organization>> CustomerModuleCreateOrganizationAsyncWithHttpInfo(Organization organization)
        {
            // verify the required parameter 'organization' is set
            if (organization == null)
                throw new ApiException(400, "Missing required parameter 'organization' when calling VirtoCommerceCustomerApi->CustomerModuleCreateOrganization");

            var localVarPath = "/api/organizations";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (organization.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(organization); // http body (model) parameter
            }
            else
            {
                localVarPostBody = organization; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleCreateOrganization: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleCreateOrganization: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Organization>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Organization)ApiClient.Deserialize(localVarResponse, typeof(Organization)));
            
        }
        /// <summary>
        /// Delete contacts Delete contacts by given array of ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of contacts ids</param>
        /// <returns></returns>
        public void CustomerModuleDeleteContacts(List<string> ids)
        {
             CustomerModuleDeleteContactsWithHttpInfo(ids);
        }

        /// <summary>
        /// Delete contacts Delete contacts by given array of ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of contacts ids</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> CustomerModuleDeleteContactsWithHttpInfo(List<string> ids)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommerceCustomerApi->CustomerModuleDeleteContacts");

            var localVarPath = "/api/contacts";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleDeleteContacts: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleDeleteContacts: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Delete contacts Delete contacts by given array of ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of contacts ids</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task CustomerModuleDeleteContactsAsync(List<string> ids)
        {
             await CustomerModuleDeleteContactsAsyncWithHttpInfo(ids);

        }

        /// <summary>
        /// Delete contacts Delete contacts by given array of ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of contacts ids</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> CustomerModuleDeleteContactsAsyncWithHttpInfo(List<string> ids)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommerceCustomerApi->CustomerModuleDeleteContacts");

            var localVarPath = "/api/contacts";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleDeleteContacts: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleDeleteContacts: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Delete members Delete members by given array of ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of members ids</param>
        /// <returns></returns>
        public void CustomerModuleDeleteMembers(List<string> ids)
        {
             CustomerModuleDeleteMembersWithHttpInfo(ids);
        }

        /// <summary>
        /// Delete members Delete members by given array of ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of members ids</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> CustomerModuleDeleteMembersWithHttpInfo(List<string> ids)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommerceCustomerApi->CustomerModuleDeleteMembers");

            var localVarPath = "/api/members";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleDeleteMembers: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleDeleteMembers: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Delete members Delete members by given array of ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of members ids</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task CustomerModuleDeleteMembersAsync(List<string> ids)
        {
             await CustomerModuleDeleteMembersAsyncWithHttpInfo(ids);

        }

        /// <summary>
        /// Delete members Delete members by given array of ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of members ids</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> CustomerModuleDeleteMembersAsyncWithHttpInfo(List<string> ids)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommerceCustomerApi->CustomerModuleDeleteMembers");

            var localVarPath = "/api/members";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleDeleteMembers: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleDeleteMembers: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Delete organizations Delete organizations by given array of ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of organizations ids</param>
        /// <returns></returns>
        public void CustomerModuleDeleteOrganizations(List<string> ids)
        {
             CustomerModuleDeleteOrganizationsWithHttpInfo(ids);
        }

        /// <summary>
        /// Delete organizations Delete organizations by given array of ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of organizations ids</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> CustomerModuleDeleteOrganizationsWithHttpInfo(List<string> ids)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommerceCustomerApi->CustomerModuleDeleteOrganizations");

            var localVarPath = "/api/organizations";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleDeleteOrganizations: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleDeleteOrganizations: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Delete organizations Delete organizations by given array of ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of organizations ids</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task CustomerModuleDeleteOrganizationsAsync(List<string> ids)
        {
             await CustomerModuleDeleteOrganizationsAsyncWithHttpInfo(ids);

        }

        /// <summary>
        /// Delete organizations Delete organizations by given array of ids.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="ids">An array of organizations ids</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> CustomerModuleDeleteOrganizationsAsyncWithHttpInfo(List<string> ids)
        {
            // verify the required parameter 'ids' is set
            if (ids == null)
                throw new ApiException(400, "Missing required parameter 'ids' when calling VirtoCommerceCustomerApi->CustomerModuleDeleteOrganizations");

            var localVarPath = "/api/organizations";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (ids != null) localVarQueryParams.Add("ids", ApiClient.ParameterToString(ids)); // query parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.DELETE, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleDeleteOrganizations: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleDeleteOrganizations: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Get contact 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Contact ID</param>
        /// <returns>Contact</returns>
        public Contact CustomerModuleGetContactById(string id)
        {
             ApiResponse<Contact> localVarResponse = CustomerModuleGetContactByIdWithHttpInfo(id);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get contact 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Contact ID</param>
        /// <returns>ApiResponse of Contact</returns>
        public ApiResponse<Contact> CustomerModuleGetContactByIdWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceCustomerApi->CustomerModuleGetContactById");

            var localVarPath = "/api/contacts/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleGetContactById: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleGetContactById: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Contact>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Contact)ApiClient.Deserialize(localVarResponse, typeof(Contact)));
            
        }

        /// <summary>
        /// Get contact 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Contact ID</param>
        /// <returns>Task of Contact</returns>
        public async System.Threading.Tasks.Task<Contact> CustomerModuleGetContactByIdAsync(string id)
        {
             ApiResponse<Contact> localVarResponse = await CustomerModuleGetContactByIdAsyncWithHttpInfo(id);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get contact 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Contact ID</param>
        /// <returns>Task of ApiResponse (Contact)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Contact>> CustomerModuleGetContactByIdAsyncWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceCustomerApi->CustomerModuleGetContactById");

            var localVarPath = "/api/contacts/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleGetContactById: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleGetContactById: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Contact>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Contact)ApiClient.Deserialize(localVarResponse, typeof(Contact)));
            
        }
        /// <summary>
        /// Get member 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">member id</param>
        /// <returns>Member</returns>
        public Member CustomerModuleGetMemberById(string id)
        {
             ApiResponse<Member> localVarResponse = CustomerModuleGetMemberByIdWithHttpInfo(id);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get member 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">member id</param>
        /// <returns>ApiResponse of Member</returns>
        public ApiResponse<Member> CustomerModuleGetMemberByIdWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceCustomerApi->CustomerModuleGetMemberById");

            var localVarPath = "/api/members/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleGetMemberById: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleGetMemberById: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Member>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Member)ApiClient.Deserialize(localVarResponse, typeof(Member)));
            
        }

        /// <summary>
        /// Get member 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">member id</param>
        /// <returns>Task of Member</returns>
        public async System.Threading.Tasks.Task<Member> CustomerModuleGetMemberByIdAsync(string id)
        {
             ApiResponse<Member> localVarResponse = await CustomerModuleGetMemberByIdAsyncWithHttpInfo(id);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get member 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">member id</param>
        /// <returns>Task of ApiResponse (Member)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Member>> CustomerModuleGetMemberByIdAsyncWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceCustomerApi->CustomerModuleGetMemberById");

            var localVarPath = "/api/members/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleGetMemberById: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleGetMemberById: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Member>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Member)ApiClient.Deserialize(localVarResponse, typeof(Member)));
            
        }
        /// <summary>
        /// Get organization 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Organization id</param>
        /// <returns>Organization</returns>
        public Organization CustomerModuleGetOrganizationById(string id)
        {
             ApiResponse<Organization> localVarResponse = CustomerModuleGetOrganizationByIdWithHttpInfo(id);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get organization 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Organization id</param>
        /// <returns>ApiResponse of Organization</returns>
        public ApiResponse<Organization> CustomerModuleGetOrganizationByIdWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceCustomerApi->CustomerModuleGetOrganizationById");

            var localVarPath = "/api/organizations/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleGetOrganizationById: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleGetOrganizationById: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Organization>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Organization)ApiClient.Deserialize(localVarResponse, typeof(Organization)));
            
        }

        /// <summary>
        /// Get organization 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Organization id</param>
        /// <returns>Task of Organization</returns>
        public async System.Threading.Tasks.Task<Organization> CustomerModuleGetOrganizationByIdAsync(string id)
        {
             ApiResponse<Organization> localVarResponse = await CustomerModuleGetOrganizationByIdAsyncWithHttpInfo(id);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get organization 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Organization id</param>
        /// <returns>Task of ApiResponse (Organization)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Organization>> CustomerModuleGetOrganizationByIdAsyncWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceCustomerApi->CustomerModuleGetOrganizationById");

            var localVarPath = "/api/organizations/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleGetOrganizationById: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleGetOrganizationById: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Organization>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Organization)ApiClient.Deserialize(localVarResponse, typeof(Organization)));
            
        }
        /// <summary>
        /// Get vendor 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Vendor ID</param>
        /// <returns>Vendor</returns>
        public Vendor CustomerModuleGetVendorById(string id)
        {
             ApiResponse<Vendor> localVarResponse = CustomerModuleGetVendorByIdWithHttpInfo(id);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get vendor 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Vendor ID</param>
        /// <returns>ApiResponse of Vendor</returns>
        public ApiResponse<Vendor> CustomerModuleGetVendorByIdWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceCustomerApi->CustomerModuleGetVendorById");

            var localVarPath = "/api/vendors/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleGetVendorById: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleGetVendorById: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Vendor>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Vendor)ApiClient.Deserialize(localVarResponse, typeof(Vendor)));
            
        }

        /// <summary>
        /// Get vendor 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Vendor ID</param>
        /// <returns>Task of Vendor</returns>
        public async System.Threading.Tasks.Task<Vendor> CustomerModuleGetVendorByIdAsync(string id)
        {
             ApiResponse<Vendor> localVarResponse = await CustomerModuleGetVendorByIdAsyncWithHttpInfo(id);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get vendor 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="id">Vendor ID</param>
        /// <returns>Task of ApiResponse (Vendor)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Vendor>> CustomerModuleGetVendorByIdAsyncWithHttpInfo(string id)
        {
            // verify the required parameter 'id' is set
            if (id == null)
                throw new ApiException(400, "Missing required parameter 'id' when calling VirtoCommerceCustomerApi->CustomerModuleGetVendorById");

            var localVarPath = "/api/vendors/{id}";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (id != null) localVarPathParams.Add("id", ApiClient.ParameterToString(id)); // path parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleGetVendorById: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleGetVendorById: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<Vendor>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Vendor)ApiClient.Deserialize(localVarResponse, typeof(Vendor)));
            
        }
        /// <summary>
        /// Get organizations Get array of all organizations.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>List&lt;Organization&gt;</returns>
        public List<Organization> CustomerModuleListOrganizations()
        {
             ApiResponse<List<Organization>> localVarResponse = CustomerModuleListOrganizationsWithHttpInfo();
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get organizations Get array of all organizations.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>ApiResponse of List&lt;Organization&gt;</returns>
        public ApiResponse<List<Organization>> CustomerModuleListOrganizationsWithHttpInfo()
        {

            var localVarPath = "/api/members/organizations";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleListOrganizations: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleListOrganizations: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Organization>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Organization>)ApiClient.Deserialize(localVarResponse, typeof(List<Organization>)));
            
        }

        /// <summary>
        /// Get organizations Get array of all organizations.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of List&lt;Organization&gt;</returns>
        public async System.Threading.Tasks.Task<List<Organization>> CustomerModuleListOrganizationsAsync()
        {
             ApiResponse<List<Organization>> localVarResponse = await CustomerModuleListOrganizationsAsyncWithHttpInfo();
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get organizations Get array of all organizations.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <returns>Task of ApiResponse (List&lt;Organization&gt;)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<List<Organization>>> CustomerModuleListOrganizationsAsyncWithHttpInfo()
        {

            var localVarPath = "/api/members/organizations";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleListOrganizations: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleListOrganizations: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<List<Organization>>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (List<Organization>)ApiClient.Deserialize(localVarResponse, typeof(List<Organization>)));
            
        }
        /// <summary>
        /// Get members Get array of members satisfied search criteria.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        /// <returns>MembersSearchResult</returns>
        public MembersSearchResult CustomerModuleSearch(MembersSearchCriteria criteria)
        {
             ApiResponse<MembersSearchResult> localVarResponse = CustomerModuleSearchWithHttpInfo(criteria);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get members Get array of members satisfied search criteria.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        /// <returns>ApiResponse of MembersSearchResult</returns>
        public ApiResponse<MembersSearchResult> CustomerModuleSearchWithHttpInfo(MembersSearchCriteria criteria)
        {
            // verify the required parameter 'criteria' is set
            if (criteria == null)
                throw new ApiException(400, "Missing required parameter 'criteria' when calling VirtoCommerceCustomerApi->CustomerModuleSearch");

            var localVarPath = "/api/members/search";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (criteria.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(criteria); // http body (model) parameter
            }
            else
            {
                localVarPostBody = criteria; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleSearch: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleSearch: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<MembersSearchResult>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (MembersSearchResult)ApiClient.Deserialize(localVarResponse, typeof(MembersSearchResult)));
            
        }

        /// <summary>
        /// Get members Get array of members satisfied search criteria.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        /// <returns>Task of MembersSearchResult</returns>
        public async System.Threading.Tasks.Task<MembersSearchResult> CustomerModuleSearchAsync(MembersSearchCriteria criteria)
        {
             ApiResponse<MembersSearchResult> localVarResponse = await CustomerModuleSearchAsyncWithHttpInfo(criteria);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get members Get array of members satisfied search criteria.
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="criteria">concrete instance of SearchCriteria type type will be created by using PolymorphicMemberSearchCriteriaJsonConverter</param>
        /// <returns>Task of ApiResponse (MembersSearchResult)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<MembersSearchResult>> CustomerModuleSearchAsyncWithHttpInfo(MembersSearchCriteria criteria)
        {
            // verify the required parameter 'criteria' is set
            if (criteria == null)
                throw new ApiException(400, "Missing required parameter 'criteria' when calling VirtoCommerceCustomerApi->CustomerModuleSearch");

            var localVarPath = "/api/members/search";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml"
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (criteria.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(criteria); // http body (model) parameter
            }
            else
            {
                localVarPostBody = criteria; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleSearch: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleSearch: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            return new ApiResponse<MembersSearchResult>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (MembersSearchResult)ApiClient.Deserialize(localVarResponse, typeof(MembersSearchResult)));
            
        }
        /// <summary>
        /// Update contact 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="contact"></param>
        /// <returns></returns>
        public void CustomerModuleUpdateContact(Contact contact)
        {
             CustomerModuleUpdateContactWithHttpInfo(contact);
        }

        /// <summary>
        /// Update contact 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="contact"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> CustomerModuleUpdateContactWithHttpInfo(Contact contact)
        {
            // verify the required parameter 'contact' is set
            if (contact == null)
                throw new ApiException(400, "Missing required parameter 'contact' when calling VirtoCommerceCustomerApi->CustomerModuleUpdateContact");

            var localVarPath = "/api/contacts";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (contact.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(contact); // http body (model) parameter
            }
            else
            {
                localVarPostBody = contact; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.PUT, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleUpdateContact: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleUpdateContact: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Update contact 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="contact"></param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task CustomerModuleUpdateContactAsync(Contact contact)
        {
             await CustomerModuleUpdateContactAsyncWithHttpInfo(contact);

        }

        /// <summary>
        /// Update contact 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="contact"></param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> CustomerModuleUpdateContactAsyncWithHttpInfo(Contact contact)
        {
            // verify the required parameter 'contact' is set
            if (contact == null)
                throw new ApiException(400, "Missing required parameter 'contact' when calling VirtoCommerceCustomerApi->CustomerModuleUpdateContact");

            var localVarPath = "/api/contacts";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (contact.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(contact); // http body (model) parameter
            }
            else
            {
                localVarPostBody = contact; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.PUT, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleUpdateContact: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleUpdateContact: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Update member 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        /// <returns></returns>
        public void CustomerModuleUpdateMember(Member member)
        {
             CustomerModuleUpdateMemberWithHttpInfo(member);
        }

        /// <summary>
        /// Update member 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> CustomerModuleUpdateMemberWithHttpInfo(Member member)
        {
            // verify the required parameter 'member' is set
            if (member == null)
                throw new ApiException(400, "Missing required parameter 'member' when calling VirtoCommerceCustomerApi->CustomerModuleUpdateMember");

            var localVarPath = "/api/members";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (member.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(member); // http body (model) parameter
            }
            else
            {
                localVarPostBody = member; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.PUT, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleUpdateMember: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleUpdateMember: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Update member 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task CustomerModuleUpdateMemberAsync(Member member)
        {
             await CustomerModuleUpdateMemberAsyncWithHttpInfo(member);

        }

        /// <summary>
        /// Update member 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="member">concrete instance of abstract member type will be created by using PolymorphicMemberJsonConverter</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> CustomerModuleUpdateMemberAsyncWithHttpInfo(Member member)
        {
            // verify the required parameter 'member' is set
            if (member == null)
                throw new ApiException(400, "Missing required parameter 'member' when calling VirtoCommerceCustomerApi->CustomerModuleUpdateMember");

            var localVarPath = "/api/members";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (member.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(member); // http body (model) parameter
            }
            else
            {
                localVarPostBody = member; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.PUT, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleUpdateMember: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleUpdateMember: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
        /// <summary>
        /// Update organization 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="organization"></param>
        /// <returns></returns>
        public void CustomerModuleUpdateOrganization(Organization organization)
        {
             CustomerModuleUpdateOrganizationWithHttpInfo(organization);
        }

        /// <summary>
        /// Update organization 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="organization"></param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> CustomerModuleUpdateOrganizationWithHttpInfo(Organization organization)
        {
            // verify the required parameter 'organization' is set
            if (organization == null)
                throw new ApiException(400, "Missing required parameter 'organization' when calling VirtoCommerceCustomerApi->CustomerModuleUpdateOrganization");

            var localVarPath = "/api/organizations";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (organization.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(organization); // http body (model) parameter
            }
            else
            {
                localVarPostBody = organization; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)ApiClient.CallApi(localVarPath,
                Method.PUT, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleUpdateOrganization: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleUpdateOrganization: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Update organization 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="organization"></param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task CustomerModuleUpdateOrganizationAsync(Organization organization)
        {
             await CustomerModuleUpdateOrganizationAsyncWithHttpInfo(organization);

        }

        /// <summary>
        /// Update organization 
        /// </summary>
        /// <exception cref="VirtoCommerce.CustomerModule.Client.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="organization"></param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> CustomerModuleUpdateOrganizationAsyncWithHttpInfo(Organization organization)
        {
            // verify the required parameter 'organization' is set
            if (organization == null)
                throw new ApiException(400, "Missing required parameter 'organization' when calling VirtoCommerceCustomerApi->CustomerModuleUpdateOrganization");

            var localVarPath = "/api/organizations";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new Dictionary<string, string>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            string[] localVarHttpContentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/xml", 
                "text/xml", 
                "application/x-www-form-urlencoded"
            };
            string localVarHttpContentType = ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            string[] localVarHttpHeaderAccepts = new string[] {
            };
            string localVarHttpHeaderAccept = ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            // set "format" to json by default
            // e.g. /pet/{petId}.{format} becomes /pet/{petId}.json
            localVarPathParams.Add("format", "json");
            if (organization.GetType() != typeof(byte[]))
            {
                localVarPostBody = ApiClient.Serialize(organization); // http body (model) parameter
            }
            else
            {
                localVarPostBody = organization; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse)await ApiClient.CallApiAsync(localVarPath,
                Method.PUT, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int)localVarResponse.StatusCode;

            if (localVarStatusCode >= 400 && (localVarStatusCode != 404 || Configuration.ThrowExceptionWhenStatusCodeIs404))
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleUpdateOrganization: " + localVarResponse.Content, localVarResponse.Content);
            else if (localVarStatusCode == 0)
                throw new ApiException(localVarStatusCode, "Error calling CustomerModuleUpdateOrganization: " + localVarResponse.ErrorMessage, localVarResponse.ErrorMessage);

            
            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }
    }
}
