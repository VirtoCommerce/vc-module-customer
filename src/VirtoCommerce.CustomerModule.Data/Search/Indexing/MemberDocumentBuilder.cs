using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CustomerModule.Data.Search.Indexing
{
    public class MemberDocumentBuilder : IIndexSchemaBuilder, IIndexDocumentBuilder
    {
        private readonly IMemberService _memberService;
        private readonly IDynamicPropertySearchService _dynamicPropertySearchService;

        public MemberDocumentBuilder(IMemberService memberService, IDynamicPropertySearchService dynamicPropertySearchService)
        {
            _memberService = memberService;
            _dynamicPropertySearchService = dynamicPropertySearchService;
        }

        public Task BuildSchemaAsync(IndexDocument schema)
        {
            // Common fields
            schema.AddFilterableString("MemberType");
            schema.AddFilterableString("OuterId");

            schema.AddFilterableStringAndContentString("Name");
            schema.AddFilterableStringAndContentString("Status");

            schema.AddFilterableCollectionAndContentString("Emails");
            schema.AddFilterableCollectionAndContentString("Phones");

            schema.AddFilterableCollection("Groups");

            schema.AddFilterableDateTime("CreatedDate");
            schema.AddFilterableDateTime("ModifiedDate");

            // Contact
            schema.AddFilterableStringAndContentString("Salutation");
            schema.AddFilterableStringAndContentString("FullName");
            schema.AddFilterableStringAndContentString("FirstName");
            schema.AddFilterableStringAndContentString("MiddleName");
            schema.AddFilterableStringAndContentString("LastName");
            schema.AddFilterableDateTime("BirthDate");
            schema.AddFilterableString("DefaultLanguage");
            schema.AddFilterableString("TimeZone");

            schema.AddFilterableString("TaxpayerId");
            schema.AddFilterableString("PreferredDelivery");
            schema.AddFilterableString("PreferredCommunication");

            schema.AddFilterableCollectionAndContentString("Login");
            schema.AddFilterableBoolean("IsAnonymized");
            schema.AddFilterableStringAndContentString("About");

            schema.AddFilterableCollection("Role");
            schema.AddFilterableCollection("RoleId");

            // Employee
            schema.AddFilterableString("EmployeeType");
            schema.AddFilterableBoolean("IsActive");

            // Organization
            schema.AddFilterableString("BusinessCategory");
            schema.AddFilterableString("OwnerId");

            // Vendor
            schema.AddFilterableString("GroupName");

            // Parent organizations
            schema.AddFilterableCollection("ParentOrganizations");
            schema.AddFilterableBoolean("HasParentOrganizations");

            // Associated organizations
            schema.AddFilterableCollection("AssociatedOrganizations");
            schema.AddFilterableBoolean("HasAssociatedOrganizations");

            return schema.AddDynamicProperties(_dynamicPropertySearchService,
                typeof(Contact).FullName,
                typeof(Employee).FullName,
                typeof(Organization).FullName,
                typeof(Vendor).FullName);
        }

        public virtual async Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            var members = await GetMembers(documentIds);
            var result = new List<IndexDocument>();

            foreach (var member in members)
            {
                result.Add(await CreateDocument(member));
            }

            return result;
        }

        protected virtual Task<Member[]> GetMembers(IList<string> documentIds)
        {
            return _memberService.GetByIdsAsync(documentIds.ToArray());
        }

        protected virtual async Task<IndexDocument> CreateDocument(Member member)
        {
            var document = new IndexDocument(member.Id);

            document.AddFilterableString("MemberType", member.MemberType);
            document.AddFilterableString("OuterId", member.OuterId);

            document.AddFilterableStringAndContentString("Name", member.Name);
            document.AddFilterableCollectionAndContentString("Emails", member.Emails);
            document.AddFilterableCollectionAndContentString("Phones", member.Phones);
            document.AddFilterableStringAndContentString("Status", member.Status);
            document.AddFilterableCollection("Groups", member.Groups);

            document.AddFilterableDateTime("CreatedDate", member.CreatedDate);
            document.AddFilterableDateTime("ModifiedDate", member.ModifiedDate ?? member.CreatedDate);

            if (member.Addresses?.Any() == true)
            {
                foreach (var address in member.Addresses)
                {
                    IndexAddress(document, address);
                }
            }

            if (member.Notes?.Any() == true)
            {
                foreach (var note in member.Notes)
                {
                    IndexNote(document, note);
                }
            }

            var contact = member as Contact;
            var employee = member as Employee;
            var organization = member as Organization;
            var vendor = member as Vendor;

            if (contact != null)
            {
                IndexContact(document, contact);
            }
            else if (employee != null)
            {
                IndexEmployee(document, employee);
            }
            else if (organization != null)
            {
                IndexOrganization(document, organization);
            }
            else if (vendor != null)
            {
                IndexVendor(document, vendor);
            }

#pragma warning disable VC0005 // Type or member is obsolete
            await IndexDynamicProperties(member, document);
#pragma warning restore VC0005 // Type or member is obsolete

            return document;
        }

        protected virtual void IndexAddress(IndexDocument document, Address address)
        {
            document.AddContentString(address.AddressType.ToString());
            document.AddContentString(address.Name);
            document.AddContentString(address.Organization);
            document.AddContentString(address.CountryCode);
            document.AddContentString(address.CountryName);
            document.AddContentString(address.City);
            document.AddContentString(address.PostalCode);
            document.AddContentString(address.Zip);
            document.AddContentString(address.Line1);
            document.AddContentString(address.Line2);
            document.AddContentString(address.RegionId);
            document.AddContentString(address.RegionName);
            document.AddContentString(address.FirstName);
            document.AddContentString(address.MiddleName);
            document.AddContentString(address.LastName);
            document.AddContentString(address.Phone);
            document.AddContentString(address.Email);
            document.AddContentString(address.OuterId);
        }

        protected virtual void IndexNote(IndexDocument document, Note note)
        {
            document.AddContentString(note.Title);
            document.AddContentString(note.Body);
        }

        protected virtual void IndexContact(IndexDocument document, Contact contact)
        {
            document.AddFilterableStringAndContentString("Salutation", contact.Salutation);
            document.AddFilterableStringAndContentString("FullName", contact.FullName);
            document.AddFilterableStringAndContentString("FirstName", contact.FirstName);
            document.AddFilterableStringAndContentString("MiddleName", contact.MiddleName);
            document.AddFilterableStringAndContentString("LastName", contact.LastName);
            document.AddFilterableDateTime("BirthDate", contact.BirthDate);
            document.AddFilterableString("DefaultLanguage", contact.DefaultLanguage);
            document.AddFilterableString("TimeZone", contact.TimeZone);
            AddParentOrganizations(document, contact.Organizations);
            AddAssociatedOrganizations(document, contact.AssociatedOrganizations);

            document.AddFilterableString("TaxpayerId", contact.TaxPayerId);
            document.AddFilterableString("PreferredDelivery", contact.PreferredDelivery);
            document.AddFilterableString("PreferredCommunication", contact.PreferredCommunication);

            document.AddFilterableCollectionAndContentString("Login", contact.SecurityAccounts.Select(sa => sa.UserName).ToList());
            document.AddFilterableBoolean("IsAnonymized", contact.IsAnonymized);
            document.AddFilterableStringAndContentString("About", contact.About);

            document.AddFilterableCollection("Role", contact
                .SecurityAccounts
                .SelectMany(x => x.Roles ?? Enumerable.Empty<Role>())
                .Select(x => x.NormalizedName)
                .ToList());
            document.AddFilterableCollection("RoleId", contact
                .SecurityAccounts
                .SelectMany(x => x.Roles ?? Enumerable.Empty<Role>())
                .Select(x => x.Id)
                .ToList());
        }

        protected virtual void IndexEmployee(IndexDocument document, Employee employee)
        {
            document.AddFilterableStringAndContentString("Salutation", employee.Salutation);
            document.AddFilterableStringAndContentString("FullName", employee.FullName);
            document.AddFilterableStringAndContentString("FirstName", employee.FirstName);
            document.AddFilterableStringAndContentString("MiddleName", employee.MiddleName);
            document.AddFilterableStringAndContentString("LastName", employee.LastName);
            document.AddFilterableDateTime("BirthDate", employee.BirthDate);
            AddParentOrganizations(document, employee.Organizations);

            document.AddFilterableString("EmployeeType", employee.EmployeeType);
            document.AddFilterableBoolean("IsActive", employee.IsActive);
        }

        protected virtual void IndexOrganization(IndexDocument document, Organization organization)
        {
            document.AddContentString(organization.Description);
            document.AddFilterableString("BusinessCategory", organization.BusinessCategory);
            document.AddFilterableString("OwnerId", organization.OwnerId);
            AddParentOrganizations(document, new[] { organization.ParentId });
        }

        protected virtual void IndexVendor(IndexDocument document, Vendor vendor)
        {
            document.AddContentString(vendor.Description);
            document.AddFilterableString("GroupName", vendor.GroupName);
        }

        protected virtual void AddParentOrganizations(IndexDocument document, ICollection<string> values)
        {
            var nonEmptyValues = values?.Where(v => !string.IsNullOrEmpty(v)).ToArray();

            document.AddFilterableCollection("ParentOrganizations", nonEmptyValues);
            document.AddFilterableBoolean("HasParentOrganizations", nonEmptyValues?.Any() ?? false);
        }

        protected virtual void AddAssociatedOrganizations(IndexDocument document, ICollection<string> values)
        {
            var nonEmptyValues = values?.Where(v => !string.IsNullOrEmpty(v)).ToArray();

            document.AddFilterableCollection("AssociatedOrganizations", nonEmptyValues);
            document.AddFilterableBoolean("HasAssociatedOrganizations", nonEmptyValues?.Any() ?? false);
        }

        [Obsolete("Use IndexDocument.AddDynamicProperties() extension method", DiagnosticId = "VC0005", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        protected virtual async Task IndexDynamicProperties(Member member, IndexDocument document)
        {
            var properties = await _dynamicPropertySearchService.GetAllDynamicProperties(member.ObjectType);

            foreach (var property in properties)
            {
                var objectProperty = member.DynamicProperties?.FirstOrDefault(x => x.Id == property.Id) ??
                    member.DynamicProperties?.FirstOrDefault(x => x.Name.EqualsInvariant(property.Name) && x.HasValuesOfType(property.ValueType));

                IndexDynamicProperty(document, property, objectProperty);
            }
        }

        [Obsolete("Use IndexDocument.AddDynamicProperty() extension method", DiagnosticId = "VC0005", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions/")]
        protected virtual void IndexDynamicProperty(IndexDocument document, DynamicProperty property, DynamicObjectProperty objectProperty)
        {
            document.AddDynamicProperty(property, objectProperty);
        }
    }
}
