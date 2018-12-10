using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoCompare;
using VirtoCommerce.CustomerModule.Data.Resources;
using VirtoCommerce.Domain.Common.Events;
using VirtoCommerce.Domain.Customer.Events;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CustomerModule.Data.Handlers
{
    /// <summary>
    /// Represents logic for audit logging for contact changes.
    /// </summary>
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
            //Log audit records only for contacts members
            var operationLogs = message.ChangedEntries.Where(x => x.EntryState == EntryState.Modified && x.OldEntry is Contact)
                                        .SelectMany(GetChangedEntryOperationLogs)
                                        .ToArray();
            if (operationLogs.Any())
            {
                _changeLogService.SaveChanges(operationLogs);
            }
            return Task.CompletedTask;
        }

        protected virtual IEnumerable<OperationLog> GetChangedEntryOperationLogs(GenericChangedEntry<Member> changedEntry)
        {
            var result = new List<string>();

            var oldContact = changedEntry.OldEntry as Contact;
            var changedContact = changedEntry.NewEntry as Contact;

            var diff = Comparer.Compare(oldContact, changedContact);
            //First, we detect changes with simple properties of the contact object.
            var observedDifferences = diff.Join(_observedProperties, x => x.Name.ToLowerInvariant(), x => x.ToLowerInvariant(), (x, y) => x).ToArray();
            var anonymousDiffComparer = AnonymousComparer.Create((Difference x) => string.Join(":", x.Name, x.NewValue, x.OldValue));
            foreach (var difference in observedDifferences.Distinct(anonymousDiffComparer))
            {
                result.Add(string.Format(MemberResources.MemberPropertyChanged, difference.Name, difference.OldValue, difference.NewValue));
            }
            //Second, detect changes for contact dependencies
            result.AddRange(GetContactDependenciesChanges(oldContact, changedContact));

            return result.Select(x => GetLogRecord(changedContact, x));
        }

        protected virtual IEnumerable<string> GetContactDependenciesChanges(Contact originalContact, Contact modifiedContact)
        {
            return DetectCollectionsChanges(originalContact.Addresses, modifiedContact.Addresses, "Address")
                    .Union(DetectCollectionsChanges(originalContact.Emails, modifiedContact.Emails, "Email"))
                    .Union(DetectCollectionsChanges(originalContact.Phones, modifiedContact.Phones, "Phone"));
        }

        protected static IEnumerable<string> DetectCollectionsChanges<T>(IEnumerable<T> originalCollection, IEnumerable<T> modifiedCollection, string resourceKeyPrefix)
        {
            var result = new List<string>();
            if (originalCollection != null && modifiedCollection != null)
            {
                void CompareAction(EntryState state, T source, T target)
                {
                    //Do not log unchanged values.
                    if (state != EntryState.Modified || source?.ToString() != target?.ToString())
                    {
                        result.Add(string.Format(
                            MemberResources.ResourceManager.GetString($"{resourceKeyPrefix}{state}") ?? string.Empty, target, source));
                    }
                }
                modifiedCollection
                    .Where(x => x != null).ToList()
                    .CompareTo(originalCollection.Where(x => x != null).ToList(), EqualityComparer<T>.Default, CompareAction);
            }
            return result.Where(x => !string.IsNullOrEmpty(x));
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

}
