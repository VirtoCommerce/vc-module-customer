using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CustomerModule.Data.Search.Indexing
{
    public class MemberDocumentBuilder : IIndexDocumentBuilder
    {
        private readonly IMemberService _memberService;

        public MemberDocumentBuilder(IMemberService memberService)
        {
            _memberService = memberService;
        }

        public Task<IList<IndexDocument>> GetDocumentsAsync(IList<string> documentIds)
        {
            var members = GetMembers(documentIds);

            IList<IndexDocument> result = members.Select(CreateDocument).ToArray();
            return Task.FromResult(result);
        }


        protected virtual IList<Member> GetMembers(IList<string> documentIds)
        {
            return _memberService.GetByIds(documentIds.ToArray());
        }

        protected virtual IndexDocument CreateDocument(Member member)
        {
            var document = new IndexDocument(member.Id);

            document.AddFilterableValue("MemberType", member.MemberType);
            document.AddFilterableAndSearchableValue("Name", member.Name);
            document.AddFilterableAndSearchableValues("Emails", member.Emails);
            document.AddFilterableAndSearchableValues("Phones", member.Phones);
            document.AddFilterableValues("Groups", member.Groups);

            document.AddFilterableValue("CreatedDate", member.CreatedDate);
            document.AddFilterableValue("ModifiedDate", member.ModifiedDate ?? member.CreatedDate);

            if (member.Addresses != null)
            {
                // TODO: Index addresses
            }

            if (member.Notes != null)
            {
                // TODO: Index notes
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

            return document;
        }

        protected virtual void IndexContact(IndexDocument document, Contact contact)
        {
            document.AddFilterableAndSearchableValue("Salutation", contact.Salutation);

            document.AddFilterableAndSearchableValue("FullName", contact.FullName);
            document.AddFilterableAndSearchableValue("FirstName", contact.FirstName);
            document.AddFilterableAndSearchableValue("MiddleName", contact.MiddleName);
            document.AddFilterableAndSearchableValue("LastName", contact.LastName);
            document.AddFilterableValue("BirthDate", contact.BirthDate);
            document.AddFilterableValues("Organizations", contact.Organizations);

            document.AddFilterableValue("TaxpayerId", contact.TaxpayerId);
            document.AddFilterableValue("PreferredDelivery", contact.PreferredDelivery);
            document.AddFilterableValue("PreferredCommunication", contact.PreferredCommunication);
        }

        protected virtual void IndexEmployee(IndexDocument document, Employee employee)
        {
            document.AddFilterableAndSearchableValue("FullName", employee.FullName);
            document.AddFilterableAndSearchableValue("FirstName", employee.FirstName);
            document.AddFilterableAndSearchableValue("MiddleName", employee.MiddleName);
            document.AddFilterableAndSearchableValue("LastName", employee.LastName);
            document.AddFilterableValue("BirthDate", employee.BirthDate);
            document.AddFilterableValues("Organizations", employee.Organizations);

            document.AddFilterableValue("EmployeeType", employee.Type);
            document.AddFilterableValue("IsActive", employee.IsActive);
        }

        protected virtual void IndexOrganization(IndexDocument document, Organization organization)
        {
            document.AddSearchableValue(organization.Description);
            document.AddFilterableValue("BusinessCategory", organization.BusinessCategory);
            document.AddFilterableValue("OwnerId", organization.OwnerId);
            document.AddFilterableValue("ParentId", organization.ParentId);
        }

        protected virtual void IndexVendor(IndexDocument document, Vendor vendor)
        {
            document.AddSearchableValue(vendor.Description);
            document.AddFilterableValue("GroupName", vendor.GroupName);
        }
    }
}
