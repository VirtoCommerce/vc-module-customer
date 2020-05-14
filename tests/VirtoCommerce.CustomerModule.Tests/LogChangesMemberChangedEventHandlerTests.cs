using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Moq;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Data.Handlers;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using Xunit;

namespace VirtoCommerce.CustomerModule.Tests
{
    [Trait("Category", "CI")]
    [Trait("Category", "IntegrationTest")]
    public class LogChangesMemberChangedEventHandlerTests
    {
        [Theory]
        [MemberData(nameof(LogsData))]
        [CLSCompliant(false)]
        public async Task LogChangesMemberChangedEventHandler_SavesChanges(
            Contact oldMember, Contact newMember, IReadOnlyCollection<string> expectedLogs)
        {
            JobStorage.Current = new Mock<JobStorage>().Object;
            var changesLogService = new Mock<IChangeLogService>();
            changesLogService
                .Setup(x => x.SaveChangesAsync(It.IsAny<OperationLog[]>()))
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
            var handler = new LogChangesEventHandler(changesLogService.Object);

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
                      string.Format("The property '{0}' changed from '{1}' to '{2}'", "Name", "Name1", "Name_New"),
                      string.Format("The property '{0}' changed from '{1}' to '{2}'", "FirstName", "FirstName1", "FirstName_NEW"),
                      string.Format("The property '{0}' changed from '{1}' to '{2}'", "MiddleName", "MiddleName1", "MiddleName_NEW"),
                      string.Format("The property '{0}' changed from '{1}' to '{2}'", "LastName", "LastName1", "LastName_NEW"),
                      string.Format("The property '{0}' changed from '{1}' to '{2}'", "Salutation", "Salutation1", "Salutation_NEW"),
                      string.Format("The property '{0}' changed from '{1}' to '{2}'", "FullName", "FullName1", "FullName_NEW"),
                      string.Format("The property '{0}' changed from '{1}' to '{2}'", "BirthDate", new DateTime(2001, 1, 10), new DateTime(2001, 3, 10)),
                    }
                },             
                // Emails
                {
                    new Contact { Id = "id", Emails = new List<string>() { "unchanged@mail.com", "deleted@mail.com" } },
                    new Contact { Id = "id", Emails = new List<string> { "unchanged@mail.com", "added@mail.com" } },
                    new[]
                    {
                        string.Format("The address '{0}' added", "added@mail.com"),
                        string.Format("The address '{0}' deleted", "deleted@mail.com")
                    }
                },           
                // Phones
                {
                    new Contact { Id = "id", Phones = new List<string>(){ "unchanged phone", "deleted phone" } },
                    new Contact { Id = "id", Phones = new List<string> { "unchanged phone", "added phone" } },
                    new[]
                    {
                       string.Format("The phone '{0}' added", "added phone"),
                       string.Format("The phone '{0}' deleted", "deleted phone")
                    }
                },             
                // Addresses
                {
                   new Contact { Id = "id", Addresses = new List<Address>() { new Address { Key = "1", City = "modified address" }, new Address { Key = "2", City = "deleted address" } } },
                   new Contact { Id = "id", Addresses = new List<Address> {  new Address { City = "added address" }, new Address { Key = "1", City = "modified address2" } } },
                   new[]
                   {
                       string.Format("The address '{0}' added", "added address"),
                       string.Format("The address '{0}' deleted", "deleted address"),
                       string.Format("The address  '{0}' changed to '{1}'", "modified address", "modified address2")
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
