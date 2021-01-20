using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.SearchModule.Core.Extenstions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.Search.Indexing
{
    public class MemberDocumentBuilder : IIndexDocumentBuilder
    {
        public static readonly string NoValueString = "__null";
        private readonly IMemberService _memberService;
        private readonly IDynamicPropertySearchService _dynamicPropertySearchService;

        public MemberDocumentBuilder(IMemberService memberService, IDynamicPropertySearchService dynamicPropertySearchService)
        {
            _memberService = memberService;
            _dynamicPropertySearchService = dynamicPropertySearchService;
        }

        public virtual async Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            var members = await GetMembers(documentIds);

            IList<IndexDocument> result = members.Select(CreateDocument).ToArray();
            return result;
        }

        protected virtual async Task<Member[]> GetMembers(IList<string> documentIds)
        {
            var result = await _memberService.GetByIdsAsync(documentIds.ToArray());
            await LoadDependencies(result);
            return result;
        }

        protected virtual IndexDocument CreateDocument(Member member)
        {
            var document = new IndexDocument(member.Id);

            document.AddFilterableValue("MemberType", member.MemberType);
            document.AddFilterableAndSearchableValue("Name", member.Name);
            document.AddFilterableAndSearchableValues("Emails", member.Emails);
            document.AddFilterableAndSearchableValues("Phones", member.Phones);
            document.AddFilterableAndSearchableValue("Status", member.Status);
            document.AddFilterableValues("Groups", member.Groups);

            document.AddFilterableValue("CreatedDate", member.CreatedDate);
            document.AddFilterableValue("ModifiedDate", member.ModifiedDate ?? member.CreatedDate);

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
            if (!member.DynamicProperties.IsNullOrEmpty())
            {
                foreach (var property in member.DynamicProperties)
                {
                    IndexDynamicProperty(document, property);
                }
            }

            return document;
        }

        protected virtual void IndexAddress(IndexDocument document, Address address)
        {
            document.AddSearchableValue(address.AddressType.ToString());
            document.AddSearchableValue(address.Name);
            document.AddSearchableValue(address.Organization);
            document.AddSearchableValue(address.CountryCode);
            document.AddSearchableValue(address.CountryName);
            document.AddSearchableValue(address.City);
            document.AddSearchableValue(address.PostalCode);
            document.AddSearchableValue(address.Zip);
            document.AddSearchableValue(address.Line1);
            document.AddSearchableValue(address.Line2);
            document.AddSearchableValue(address.RegionId);
            document.AddSearchableValue(address.RegionName);
            document.AddSearchableValue(address.FirstName);
            document.AddSearchableValue(address.MiddleName);
            document.AddSearchableValue(address.LastName);
            document.AddSearchableValue(address.Phone);
            document.AddSearchableValue(address.Email);
        }

        protected virtual void IndexNote(IndexDocument document, Note note)
        {
            document.AddSearchableValue(note.Title);
            document.AddSearchableValue(note.Body);
        }

        protected virtual void IndexContact(IndexDocument document, Contact contact)
        {
            document.AddFilterableAndSearchableValue("Salutation", contact.Salutation);
            document.AddFilterableAndSearchableValue("FullName", contact.FullName);
            document.AddFilterableAndSearchableValue("FirstName", contact.FirstName);
            document.AddFilterableAndSearchableValue("MiddleName", contact.MiddleName);
            document.AddFilterableAndSearchableValue("LastName", contact.LastName);
            document.AddFilterableValue("BirthDate", contact.BirthDate);
            AddParentOrganizations(document, contact.Organizations);

            document.AddFilterableValue("TaxpayerId", contact.TaxPayerId);
            document.AddFilterableValue("PreferredDelivery", contact.PreferredDelivery);
            document.AddFilterableValue("PreferredCommunication", contact.PreferredCommunication);
        }

        protected virtual void IndexEmployee(IndexDocument document, Employee employee)
        {
            document.AddFilterableAndSearchableValue("Salutation", employee.Salutation);
            document.AddFilterableAndSearchableValue("FullName", employee.FullName);
            document.AddFilterableAndSearchableValue("FirstName", employee.FirstName);
            document.AddFilterableAndSearchableValue("MiddleName", employee.MiddleName);
            document.AddFilterableAndSearchableValue("LastName", employee.LastName);
            document.AddFilterableValue("BirthDate", employee.BirthDate);
            AddParentOrganizations(document, employee.Organizations);

            document.AddFilterableValue("EmployeeType", employee.EmployeeType);
            document.AddFilterableValue("IsActive", employee.IsActive);
        }

        protected virtual void IndexOrganization(IndexDocument document, Organization organization)
        {
            document.AddSearchableValue(organization.Description);
            document.AddFilterableValue("BusinessCategory", organization.BusinessCategory);
            document.AddFilterableValue("OwnerId", organization.OwnerId);
            AddParentOrganizations(document, new[] { organization.ParentId });
        }

        protected virtual void IndexVendor(IndexDocument document, Vendor vendor)
        {
            document.AddSearchableValue(vendor.Description);
            document.AddFilterableValue("GroupName", vendor.GroupName);
        }

        protected virtual void AddParentOrganizations(IndexDocument document, ICollection<string> values)
        {
            var nonEmptyValues = values?.Where(v => !string.IsNullOrEmpty(v)).ToArray();

            document.AddFilterableValues("ParentOrganizations", nonEmptyValues);
            document.AddFilterableValue("HasParentOrganizations", nonEmptyValues?.Any() ?? false);
        }

        protected virtual void IndexDynamicProperty(IndexDocument document, DynamicObjectProperty property)
        {
            var propertyName = property.Name?.ToLowerInvariant();

            if (!string.IsNullOrEmpty(propertyName))
            {
                var isCollection = property.IsDictionary || property.IsArray;
                IList<object> values;

                if (!property.IsDictionary)
                {
                    values = property.Values.Where(x => x.Value != null)
                        .Select(x => x.Value)
                        .ToList();
                }
                else
                {
                    //add all locales in dictionary to searchIndex
                    values = property.Values.Select(x => x.Value)
                                            .Cast<DynamicPropertyDictionaryItem>()
                                            .Where(x => !string.IsNullOrEmpty(x.Name))
                                            .Select(x => x.Name)
                                            .ToList<object>();
                }

                if (values.Any())
                {
                    document.Add(new IndexDocumentField(propertyName, values) { IsRetrievable = true, IsFilterable = true, IsCollection = isCollection });
                }
            }
        }

        private async Task LoadDependencies(Member[] members)
        {
            //Load Dynamic Properties
            var criteria = AbstractTypeFactory<DynamicPropertySearchCriteria>.TryCreateInstance();
            criteria.ObjectTypes = members.Select(x => x.GetType().FullName).Distinct().ToArray();
            criteria.Take = int.MaxValue;

            var searchResult = await _dynamicPropertySearchService.SearchDynamicPropertiesAsync(criteria);
            var comparer = AnonymousComparer.Create((DynamicProperty x) => x.Id);

            if (!searchResult.Results.IsNullOrEmpty())
            {
                foreach (var member in members)
                {
                    // Use default or empty value for the property in index to be able to filter by it
                    var exceptDynamicProperties = searchResult.Results.Except(member.DynamicProperties, comparer)
                        .Select(x => FillDynamicPropertyMetaData(AbstractTypeFactory<DynamicObjectProperty>.TryCreateInstance(), x)).ToList();
                    member.DynamicProperties = member.DynamicProperties.Union(exceptDynamicProperties).ToArray();
                }
            }
        }

        private DynamicObjectProperty FillDynamicPropertyMetaData(DynamicObjectProperty objectProperty, DynamicProperty property)
        {
            var propertyName = property.Name?.ToLowerInvariant();

            if(!string.IsNullOrEmpty(propertyName))
            {
                objectProperty.Id = property.Id;
                objectProperty.Name = propertyName;
                objectProperty.ValueType = property.ValueType;

                if (objectProperty.Values.IsNullOrEmpty())
                {
                    objectProperty.Values = new[] { new DynamicPropertyObjectValue{ Value = property.IsRequired ?
                        GetDynamicPropertyDefaultValue(property)
                        ?? NoValueString : NoValueString }};
                }
            }

            return objectProperty;
        }

        private object GetDynamicPropertyDefaultValue(DynamicProperty property)
        {
            object result;

            switch (property.ValueType)
            {
                case DynamicPropertyValueType.ShortText:
                case DynamicPropertyValueType.Html:
                case DynamicPropertyValueType.LongText:
                case DynamicPropertyValueType.Image:
                    result = default(string);
                    break;

                case DynamicPropertyValueType.Integer:
                    result = default(int);
                    break;

                case DynamicPropertyValueType.Decimal:
                    result = default(decimal);
                    break;

                case DynamicPropertyValueType.DateTime:
                    result = default(DateTime);
                    break;

                case DynamicPropertyValueType.Boolean:
                    result = default(bool);
                    break;

                default:
                    result = default(object);
                    break;
            }

            return result;
        }
    }
}
