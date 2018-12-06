using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoCompare;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Domain.Customer.Events;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CustomerModule.Data.Handlers
{
    public class LogChangesMemberChangedEventHandler : IEventHandler<MemberChangedEvent>
    {
        private readonly IChangeLogService _changeLogService;
        private readonly string[] _observedProperties;

        public static string[] ObservedProperties()
        {
            var memberPropsNames = ReflectionUtility.GetPropertyNames<Contact>(
                x => x.Name, x => x.FirstName, x => x.LastName, x => x.MiddleName,
                x => x.Salutation, x => x.FullName, x => x.BirthDate);
            return memberPropsNames.ToArray();
        }

        public LogChangesMemberChangedEventHandler(IChangeLogService changeLogService)
            : this(changeLogService, ObservedProperties())
        {
        }

        protected LogChangesMemberChangedEventHandler(IChangeLogService changeLogService, string[] observedProperties)
        {
            _changeLogService = changeLogService;
            _observedProperties = observedProperties;
        }

        public virtual Task Handle(MemberChangedEvent message)
        {
            var operationLogs =
                message.ChangedEntries
                    .Where(x => x.EntryState == EntryState.Modified && x.OldEntry is Contact)
                    .SelectMany(GetChangedEntryOperationLogs)
                    .ToArray();
            _changeLogService.SaveChanges(operationLogs);
            return Task.CompletedTask;
        }

        protected virtual IEnumerable<OperationLog> GetChangedEntryOperationLogs(
            GenericChangedEntry<Member> changedEntry)
        {
            var result = new List<string>();

            var diff = Comparer.Compare(changedEntry.OldEntry, changedEntry.NewEntry);

            if (changedEntry.OldEntry is Contact contact)
            {
                result.AddRange(GetContactChanges(contact, changedEntry.NewEntry as Contact));
                diff.AddRange(Comparer.Compare(contact, changedEntry.NewEntry as Contact));
            }

            var observedDifferences =
                diff.Join(_observedProperties, x => x.Name.ToLowerInvariant(), x => x.ToLowerInvariant(), (x, y) => x)
                    .ToArray();
            foreach (var difference in observedDifferences.Distinct(new DifferenceComparer()))
            {
                AddMemberPropertyChangedOperation(result, changedEntry, difference);
            }

            return result.Select(x => GetLogRecord(changedEntry.NewEntry, x));
        }

        protected virtual IEnumerable<string> GetContactChanges(Contact originalContact, Contact modifiedContact)
        {
            return
                GetAddressChanges(
                    originalContact, originalContact.Addresses, modifiedContact.Addresses)
                    .Union(GetEmailChanges(originalContact, originalContact.Emails, modifiedContact.Emails))
                    .Union(GetPhoneChanges(originalContact, originalContact.Phones, modifiedContact.Phones));
        }

        protected virtual IEnumerable<string> GetPhoneChanges(
            Member member, IEnumerable<string> originalPhones, IEnumerable<string> modifiedPhones)
        {
            return GetListChanges(
                originalPhones,
                modifiedPhones,
                result =>
                    (state, source, target) =>
                    {
                        if (state == EntryState.Added)
                        {
                            result.Add(string.Format(MemberResources.PhoneAdded, member.MemberType, member.Name, target));
                        }
                        else if (state == EntryState.Deleted)
                        {
                            result.Add(string.Format(MemberResources.PhoneRemoved, member.MemberType, member.Name, target));
                        }
                    });
        }

        protected virtual IEnumerable<string> GetEmailChanges(
            Member member, IEnumerable<string> originalEmails, IEnumerable<string> modifiedEmails)
        {
            return GetListChanges(
                originalEmails,
                modifiedEmails,
                result =>
                    (state, source, target) =>
                    {
                        if (state == EntryState.Added)
                        {
                            result.Add(string.Format(MemberResources.EmailAdded, member.MemberType, member.Name, target));
                        }
                        else if (state == EntryState.Deleted)
                        {
                            result.Add(string.Format(MemberResources.EmailRemoved, member.MemberType, member.Name, target));
                        }
                    });
        }

        protected virtual IEnumerable<string> GetAddressChanges(
            Member member, IEnumerable<Address> originalAddress, IEnumerable<Address> modifiedAddress)
        {
            return GetListChanges(
                originalAddress,
                modifiedAddress,
                result =>
                    (state, source, target) =>
                    {
                        if (state == EntryState.Added)
                        {
                            result.Add(
                                string.Format(MemberResources.AddressAdded, member.MemberType, member.Name, StringifyAddress(target)));
                        }
                        else if (state == EntryState.Deleted)
                        {
                            result.Add(
                                string.Format(MemberResources.AddressRemoved, member.MemberType, member.Name, StringifyAddress(target)));
                        }
                    });
        }

        protected virtual IEnumerable<string> GetListChanges<T>(
            IEnumerable<T> originalCollection,
            IEnumerable<T> modifiedCollection,
            Func<List<string>, Action<EntryState, T, T>> compareAction)
        {
            if (originalCollection == null || modifiedCollection == null)
            {
                return Enumerable.Empty<string>();
            }

            var result = new List<string>();
            modifiedCollection
                .Where(x => x != null).ToList()
                .CompareTo(
                    originalCollection.Where(x => x != null).ToList(),
                    EqualityComparer<T>.Default,
                    compareAction(result));
            return result;
        }

        private static void AddMemberPropertyChangedOperation(
            List<string> result, GenericChangedEntry<Member> changedEntry, Difference difference)
        {
            result.Add(
                string.Format(
                    MemberResources.MemberPropertyChanged,
                    changedEntry.OldEntry.MemberType,
                    changedEntry.NewEntry.Name,
                    difference.Name,
                    difference.OldValue,
                    difference.NewValue));
        }

        private static string StringifyAddress(Address address)
        {
            if (address == null)
            {
                return "";
            }

            return string.Join(
                ", ",
                typeof(Address)
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .OrderBy(p => p.Name)
                    .Select(p => p.GetValue(address))
                    .Where(x => x != null));
        }

        protected virtual OperationLog GetLogRecord(Member member, string template)
        {
            var result = new OperationLog
            {
                ObjectId = member.Id,
                ObjectType = member.GetType().Name,
                OperationType = EntryState.Modified,
                Detail = template
            };
            return result;

        }
    }

    internal class DifferenceComparer : EqualityComparer<Difference>
    {
        public override bool Equals(Difference x, Difference y)
        {
            return GetHashCode(x) == GetHashCode(y);
        }

        public override int GetHashCode(Difference obj)
        {
            var result = String.Join(":", obj.Name, obj.NewValue, obj.OldValue);
            return result.GetHashCode();
        }
    }
}
