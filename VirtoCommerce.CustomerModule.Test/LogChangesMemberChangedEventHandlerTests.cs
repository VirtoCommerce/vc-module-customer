using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CustomerModule.Data;
using VirtoCommerce.CustomerModule.Data.Handlers;
using VirtoCommerce.CustomerModule.Data.Resources;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Domain.Customer.Events;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.CustomerModule.Test
{
    [Trait("Category", "CI")]
    public class LogChangesMemberChangedEventHandlerTests
    {
        [Theory]
        [MemberData(nameof(LogsData))]
        [CLSCompliant(false)]
        public async Task LogChangesMemberChangedEventHandler_SavesChanges(
            Contact oldMember, Contact newMember, IReadOnlyCollection<string> expectedLogs)
        {

            var changesLogService = new Mock<IChangeLogService>();
            changesLogService
                .Setup(x => x.SaveChanges(It.IsAny<OperationLog[]>()))
                .Callback<OperationLog[]>(logs =>
                {
                    // The amount of logged changes should be the same as expected
                    Assert.Equal(expectedLogs.Count, logs.Length);

                    // All logged changes should have:
                    Assert.All(logs, l =>
                        {
                            // The same ObjectId as oldMember.Id
                            Assert.Equal(oldMember.Id, l.ObjectId);
                            // ObjectType Contact
                            Assert.Equal("Contact", l.ObjectType);
                            // OperationType Modified
                            Assert.Equal(EntryState.Modified, l.OperationType);
                        });

                    // For each expected log there should be only one logged changed with such detail
                    foreach (var log in expectedLogs)
                    {
                        Assert.Single(logs, l => l.Detail == log);
                    }
                });
            var handler = new LogChangesMemberChangedEventHandler(changesLogService.Object);

            var message = new MemberChangedEvent(
                new[] { new GenericChangedEntry<Member>(newMember, oldMember, EntryState.Modified) });

            await handler.Handle(message);
        }

        public static TheoryData<Contact, Contact, IReadOnlyCollection<string>> LogsData()
        {
            var data = new TheoryData<Contact, Contact, IReadOnlyCollection<string>>
            {
                // Modified Name
                {
                    new Contact { Id = "id", Name = "Name1", FirstName = "FirstName1", MiddleName = "MiddleName1", LastName = "LastName1", Salutation = "Salutation1", FullName = "FullName1",  BirthDate = new DateTime(2001, 1, 10)  },
                    new Contact { Id = "id", Name = "Name_New", FirstName = "FirstName_NEW", MiddleName = "MiddleName_NEW", LastName = "LastName_NEW", Salutation = "Salutation_NEW", FullName = "FullName_NEW",  BirthDate = new DateTime(2001, 3, 10) },
                    new[]
                    {
                      string.Format(MemberResources.MemberPropertyChanged, "Name", "Name1", "Name_New"),
                      string.Format(MemberResources.MemberPropertyChanged, "FirstName", "FirstName1", "FirstName_NEW"),
                      string.Format(MemberResources.MemberPropertyChanged, "MiddleName", "MiddleName1", "MiddleName_NEW"),
                      string.Format(MemberResources.MemberPropertyChanged, "LastName", "LastName1", "LastName_NEW"),
                      string.Format(MemberResources.MemberPropertyChanged, "Salutation", "Salutation1", "Salutation_NEW"),
                      string.Format(MemberResources.MemberPropertyChanged, "FullName", "FullName1", "FullName_NEW"),
                      string.Format(MemberResources.MemberPropertyChanged, "BirthDate", new DateTime(2001, 1, 10), new DateTime(2001, 3, 10)),
                    }
                },             
                // Emails
                {
                    new Contact { Id = "id", Emails = new List<string>() { "unchanged@mail.com", "deleted@mail.com" } },
                    new Contact { Id = "id", Emails = new List<string> { "unchanged@mail.com", "added@mail.com" } },
                    new[]
                    {
                        string.Format(MemberResources.EmailAdded, "added@mail.com"),
                        string.Format(MemberResources.EmailDeleted, "deleted@mail.com")
                    }
                },           
                // Phones
                {
                    new Contact { Id = "id", Phones = new List<string>(){ "unchanged phone", "deleted phone" } },
                    new Contact { Id = "id", Phones = new List<string> { "unchanged phone", "added phone" } },
                    new[]
                    {
                       string.Format(MemberResources.PhoneAdded, "added phone"),
                       string.Format(MemberResources.PhoneDeleted, "deleted phone")
                    }
                },             
                // Addresses
                {
                   new Contact { Id = "id", Addresses = new List<Address>() { new Address { Key = "1", City = "modified address" }, new Address { Key = "2", City = "deleted address" } } },
                   new Contact { Id = "id", Addresses = new List<Address> {  new Address { City = "added address" }, new Address { Key = "1", City = "modified address2" } } },
                   new[]
                   {
                       string.Format(MemberResources.AddressAdded, "added address"),
                       string.Format(MemberResources.AddressDeleted, "deleted address"),
                       string.Format(MemberResources.AddressModified, "modified address", "modified address2")
                   }
                },
                // Partial update (when null was passed for dependencies collections
                {
                new Contact { Id = "id", Phones = new List<string>(){ "phone" }, Emails = new List<string>() { "email" }, Addresses = new List<Address>() { new Address { Name = "address" } }  },
                    new Contact { Id = "id" },
                   Array.Empty<string>()
                },
                // No changes
                {
                new Contact { Id = "id", Name = "Name" },
                    new Contact { Id = "id", Name = "Name" },
                     Array.Empty<string>()
                }
        };

            return data;
        }
    }
}
