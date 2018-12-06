using System;
using System.Collections.Generic;
using System.Linq;
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
    public class LogChangesMemberChangedEventHandlerTests
    {
        [Fact]
        public async Task LogChangesMemberChangedEventHandler_SavesChanges()
        {
            var changesLogService = new Mock<IChangeLogService>();
            var handler = new LogChangesMemberChangedEventHandler(changesLogService.Object);

            var oldMember = new Contact
            {
                Id = "ContactId1",
                Name = "Name1",
                FirstName = "FirstName1",
                MiddleName = "MiddleName1",
                LastName = "LastName1",
                Emails = new List<string> {"email1"},
                Phones = new List<string> {"phone1"},
                Salutation = "Salutation1",
                FullName = "FullName1",
                BirthDate = new DateTime(2000, 1, 1),
                MemberType = "Contact",
                Addresses = new List<Address>
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
                }
            };
            var newMember = new Contact
            {
                Id = "ContactId1",
                Name = "Name_NEW",
                FirstName = "FirstName_NEW",
                MiddleName = "MiddleName_NEW",
                LastName = "LastName_NEW",
                Emails = new List<string>{ "email_NEW" },
                Phones = new List<string>{ "phone_NEW" },
                Salutation = "Salutation_NEW",
                FullName = "FullName_NEW",
                BirthDate = new DateTime(2001, 3, 10),
                MemberType = "Contact",
                Addresses = new List<Address>{
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
                }
            };

            var message = new MemberChangedEvent(
                new[] { new GenericChangedEntry<Member>(newMember, oldMember, EntryState.Modified) })
            {
                Id = "Test Id",
                Version = 100,
                TimeStamp = DateTimeOffset.UtcNow
            };

            await handler.Handle(message);

            var newAddressString =
                "0, City_NEW, Country Code_NEW, Country_NEW, Email_NEW, FirstName_NEW_address, LastName_NEW_address, AddressLine_NEW, MiddleName_NEW_address, Address_Name_NEW, Zip_NEW";
            var oldAddressString =
                "0, City1, Country Code1, Country1, Email1, FirstName1_address, LastName1_address, AddressLine1, MiddleName1_address, Address1Name, Zip1";
            changesLogService.Verify(x => x.SaveChanges(
                It.Is<OperationLog[]>(logs =>
                    logs.Length == 13
                        && logs.Any(log =>
                            log.OperationType == EntryState.Modified
                            && log.ObjectType == "Contact"
                            && log.ObjectId == "ContactId1"
                            && log.Detail == "The Contact Name_NEW property 'Name' changed from 'Name1' to 'Name_NEW'")
                        && logs.Any(log =>
                            log.OperationType == EntryState.Modified
                            && log.ObjectType == "Contact"
                            && log.ObjectId == "ContactId1"
                            && log.Detail == "The Contact Name_NEW property 'FirstName' changed from 'FirstName1' to 'FirstName_NEW'")
                        && logs.Any(log =>
                            log.OperationType == EntryState.Modified
                            && log.ObjectType == "Contact"
                            && log.ObjectId == "ContactId1"
                            && log.Detail == "The Contact Name_NEW property 'MiddleName' changed from 'MiddleName1' to 'MiddleName_NEW'")
                        && logs.Any(log =>
                            log.OperationType == EntryState.Modified
                            && log.ObjectType == "Contact"
                            && log.ObjectId == "ContactId1"
                            && log.Detail == "The Contact Name_NEW property 'MiddleName' changed from 'MiddleName1' to 'MiddleName_NEW'")
                        && logs.Any(log =>
                            log.OperationType == EntryState.Modified
                            && log.ObjectType == "Contact"
                            && log.ObjectId == "ContactId1"
                            && log.Detail == "The Contact Name_NEW property 'Salutation' changed from 'Salutation1' to 'Salutation_NEW'")
                        && logs.Any(log =>
                            log.OperationType == EntryState.Modified
                            && log.ObjectType == "Contact"
                            && log.ObjectId == "ContactId1"
                            && log.Detail == "The Contact Name_NEW property 'FullName' changed from 'FullName1' to 'FullName_NEW'")
                        && logs.Any(log =>
                            log.OperationType == EntryState.Modified
                            && log.ObjectType == "Contact"
                            && log.ObjectId == "ContactId1"
                            && log.Detail == $"The Contact Name_NEW property 'BirthDate' changed from '{new DateTime(2000, 1, 1):G}' to '{new DateTime(2001, 3, 10):G}'")
                        && logs.Any(log =>
                            log.OperationType == EntryState.Modified
                            && log.ObjectType == "Contact"
                            && log.ObjectId == "ContactId1"
                            && log.Detail == $"The address '{newAddressString}' for Contact Name1 added")
                        && logs.Any(log =>
                            log.OperationType == EntryState.Modified
                            && log.ObjectType == "Contact"
                            && log.ObjectId == "ContactId1"
                            && log.Detail == $"The address '{oldAddressString}' for Contact Name1 deleted")
                        && logs.Any(log =>
                            log.OperationType == EntryState.Modified
                            && log.ObjectType == "Contact"
                            && log.ObjectId == "ContactId1"
                            && log.Detail == "The email 'email_NEW' for Contact Name1 added")
                        && logs.Any(log =>
                            log.OperationType == EntryState.Modified
                            && log.ObjectType == "Contact"
                            && log.ObjectId == "ContactId1"
                            && log.Detail == "The email 'email1' for Contact Name1 deleted")
                        && logs.Any(log =>
                            log.OperationType == EntryState.Modified
                            && log.ObjectType == "Contact"
                            && log.ObjectId == "ContactId1"
                            && log.Detail == "The phone 'phone_NEW' for Contact Name1 added")
                        && logs.Any(log =>
                            log.OperationType == EntryState.Modified
                            && log.ObjectType == "Contact"
                            && log.ObjectId == "ContactId1"
                            && log.Detail == "The phone 'phone1' for Contact Name1 deleted")
                                )));
        }

        [Fact]
        public async Task LogChangesMemberChangesEventHandler_DoesntChangeUnchangedProperties()
        {
            var changesLogService = new Mock<IChangeLogService>();
            var handler = new LogChangesMemberChangedEventHandler(changesLogService.Object);

            var oldMember = new Contact
            {
                Id = "ContactId1",
                Name = "Name1",
                FirstName = "FirstName1",
                MiddleName = "MiddleName1",
                LastName = "LastName1",
                Emails = new List<string> {"email1"},
                Phones = new List<string> {"phone1"},
                Salutation = "Salutation1",
                FullName = "FullName1",
                BirthDate = new DateTime(2000, 1, 1),
                MemberType = "Contact",
                Addresses = new List<Address>()
            };
            var newMember = new Contact
            {
                Id = "ContactId1",
                Name = "Name1",
                FirstName = "FirstName1",
                MiddleName = "MiddleName1",
                LastName = "LastName1",
                Emails = new List<string> {"email1"},
                Phones = new List<string> {"phone1"},
                Salutation = "Salutation1",
                FullName = "FullName1",
                BirthDate = new DateTime(2000, 1, 1),
                MemberType = "Contact",
                Addresses = new List<Address>()
            };

            var message = new MemberChangedEvent(
                new[] { new GenericChangedEntry<Member>(newMember, oldMember, EntryState.Modified) })
            {
                Id = "Test Id",
                Version = 100,
                TimeStamp = DateTimeOffset.UtcNow
            };

            await handler.Handle(message);

            changesLogService.Verify(
                x => x.SaveChanges(It.Is<OperationLog[]>(logs => logs.Length == 0)));
        }
    }
}
