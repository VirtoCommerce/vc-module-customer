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
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CustomerModule.Data.Handlers
{
    public class LogChangesMemberChangedEventHandler : IEventHandler<MemberChangedEvent>
    {
        private readonly IChangeLogService _changeLogService;
        private readonly string[] _observedProperties;

        public static string[] DefaultObservedProperties
        {
            get
            {
                var memberPropsNames = ReflectionUtility.GetPropertyNames<Contact>(
                    x => x.Name, x => x.FirstName, x => x.LastName, x => x.MiddleName,
                    x => x.Salutation, x => x.FullName, x => x.BirthDate);
                return memberPropsNames.ToArray();
            }
        }

        public LogChangesMemberChangedEventHandler(IChangeLogService changeLogService)
            : this(changeLogService, DefaultObservedProperties)
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
                GetListChanges(originalContact, originalContact.Addresses, modifiedContact.Addresses, "Address")
                    .Union(GetListChanges(originalContact, originalContact.Emails, modifiedContact.Emails, "Email"))
                    .Union(GetListChanges(originalContact, originalContact.Phones, modifiedContact.Phones, "Phone"));
        }

        protected virtual IEnumerable<string> GetListChanges<T>(
            Member member,
            IEnumerable<T> originalCollection,
            IEnumerable<T> modifiedCollection,
            string resourceKeyPrefix)
        {
            if (originalCollection == null || modifiedCollection == null)
            {
                return Enumerable.Empty<string>();
            }

            var result = new List<string>();

            void CompareAction(EntryState state, T source, T target) =>
                result.Add(string.Format(
                    MemberResources.ResourceManager.GetString($"{resourceKeyPrefix}{state}") ?? string.Empty,
                    member.MemberType, member.Name, target, source));

            modifiedCollection
                .Where(x => x != null).ToList()
                .CompareTo(originalCollection.Where(x => x != null).ToList(), EqualityComparer<T>.Default, CompareAction);
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
            if (obj == null)
            {
                return 0;
            }
            var result = String.Join(":", obj.Name, obj.NewValue, obj.OldValue);
            return result.GetHashCode();
        }
    }
}
