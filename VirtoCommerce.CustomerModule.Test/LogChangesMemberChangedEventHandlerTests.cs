using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using VirtoCommerce.CustomerModule.Data.Handlers;
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
                new[] {new GenericChangedEntry<Member>(newMember, oldMember, EntryState.Modified)});

            await handler.Handle(message);
        }

        public static TheoryData<Contact, Contact, IReadOnlyCollection<string>> LogsData()
        {
            var newAddressString =
                "0, City_NEW, Country Code_NEW, Country_NEW, Email_NEW, FirstName_NEW_address, LastName_NEW_address, AddressLine_NEW, MiddleName_NEW_address, Address_Name_NEW, Zip_NEW";
            var oldAddressString =
                "0, City1, Country Code1, Country1, Email1, FirstName1_address, LastName1_address, AddressLine1, MiddleName1_address, Address1Name, Zip1";

            var data = new TheoryData<Contact, Contact, IReadOnlyCollection<string>>
            {
                // Modified Name
                {
                    new Contact { Id = "id", Name = "Name1" },
                    new Contact { Id = "id", Name = "Name_NEW" },
                    new[] {"The Contact Name_NEW property 'Name' changed from 'Name1' to 'Name_NEW'"}
                },
                // Modified FirstName
                {
                    new Contact { Id = "id", Name = "Name", FirstName = "FirstName1" },
                    new Contact { Id = "id", Name = "Name", FirstName = "FirstName_NEW" },
                    new[] {"The Contact Name property 'FirstName' changed from 'FirstName1' to 'FirstName_NEW'"}
                },
                // Modified MiddleName
                {
                    new Contact { Id = "id", Name = "Name", MiddleName = "MiddleName1" },
                    new Contact { Id = "id", Name = "Name", MiddleName = "MiddleName_NEW" },
                    new[] {"The Contact Name property 'MiddleName' changed from 'MiddleName1' to 'MiddleName_NEW'"}
                },
                // Modified LastName
                {
                    new Contact { Id = "id", Name = "Name", LastName = "LastName1" },
                    new Contact { Id = "id", Name = "Name", LastName = "LastName_NEW" },
                    new[] {"The Contact Name property 'LastName' changed from 'LastName1' to 'LastName_NEW'"}
                },
                // Modified Salutation
                {
                    new Contact { Id = "id", Name = "Name", Salutation = "Salutation1" },
                    new Contact { Id = "id", Name = "Name", Salutation = "Salutation_NEW" },
                    new[] {"The Contact Name property 'Salutation' changed from 'Salutation1' to 'Salutation_NEW'"}
                },
                // Modified FullName
                {
                    new Contact { Id = "id", Name = "Name", FullName = "FullName1" },
                    new Contact { Id = "id", Name = "Name", FullName = "FullName_NEW" },
                    new[] {"The Contact Name property 'FullName' changed from 'FullName1' to 'FullName_NEW'"}
                },
                // Modified Birthday
                {
                    new Contact { Id = "id", Name = "Name", BirthDate = new DateTime(2000, 1, 1) },
                    new Contact { Id = "id", Name = "Name", BirthDate = new DateTime(2001, 3, 10) },
                    new[] {$"The Contact Name property 'BirthDate' changed from '{new DateTime(2000, 1, 1):G}' to '{new DateTime(2001, 3, 10):G}'"}
                },
                // Added email
                {
                    new Contact { Id = "id", Name = "Name", Emails = new List<string>() },
                    new Contact { Id = "id", Name = "Name", Emails = new List<string> {"email_NEW"} },
                    new[] {"The email 'email_NEW' for Contact Name added"}
                },
                // Deleted email
                {
                    new Contact { Id = "id", Name = "Name", Emails = new List<string> {"email1"} },
                    new Contact { Id = "id", Name = "Name", Emails = new List<string>() },
                    new[] {"The email 'email1' for Contact Name deleted"}
                },
                // Added phone
                {
                    new Contact { Id = "id", Name = "Name", Phones = new List<string>() },
                    new Contact { Id = "id", Name = "Name", Phones = new List<string>{"phone_NEW"} },
                    new[] {"The phone 'phone_NEW' for Contact Name added"}
                },
                // Deleted phone
                {
                    new Contact { Id = "id", Name = "Name", Phones = new List<string> { "phone1"} },
                    new Contact { Id = "id", Name = "Name", Phones = new List<string>() },
                    new[] {"The phone 'phone1' for Contact Name deleted"}
                },
                // Added address
                {
                    new Contact { Id = "id", Name = "Name", Addresses = new List<Address>() },
                    new Contact { Id = "id", Name = "Name", Addresses = new List<Address>
                    {
                        new Address
                        {
                            City = "City_NEW",
                            CountryCode = "Country Code_NEW",
                            CountryName = "Country_NEW",
                            Email = "Email_NEW",
                            FirstName = "FirstName_NEW_address",
                            LastName = "LastName_NEW_address",
                            Line1 = "AddressLine_NEW",
                            MiddleName = "MiddleName_NEW_address",
                            Name = "Address_Name_NEW",
                            Zip = "Zip_NEW"
                        }
                    }},
                    new[] {$"The address '{newAddressString}' for Contact Name added"}
                },
                // Deleted address
                {
                    new Contact { Id = "id", Name = "Name", Addresses = new List<Address>
                    {
                        new Address
                        {
                            City = "City1",
                            CountryCode = "Country Code1",
                            CountryName = "Country1",
                            Email = "Email1",
                            FirstName = "FirstName1_address",
                            LastName = "LastName1_address",
                            Line1 = "AddressLine1",
                            MiddleName = "MiddleName1_address",
                            Name = "Address1Name",
                            Zip = "Zip1"
                        }
                    } },
                    new Contact { Id = "id", Name = "Name", Addresses = new List<Address>() },
                    new[] {$"The address '{oldAddressString}' for Contact Name deleted"}
                },
                // Address changed
                {
                    new Contact { Id = "id", Name = "Name", Addresses = new List<Address> { new Address { City = "City1" } } },
                    new Contact { Id = "id", Name = "Name", Addresses = new List<Address> { new Address { City = "City_NEW" } } },
                    new []
                    {
                        "The address '0, City1' for Contact Name deleted",
                        "The address '0, City_NEW' for Contact Name added"
                    }
                },
                // No changes
                {
                    new Contact{ Id = "id", Name = "Name" },
                    new Contact{ Id = "id", Name = "Name" },
                    new string[0]
                }
            };

            return data;
        }
    }
}
