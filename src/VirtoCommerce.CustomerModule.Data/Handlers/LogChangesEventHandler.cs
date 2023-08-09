using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security.Events;

namespace VirtoCommerce.CustomerModule.Data.Handlers
{
    public class LogChangesEventHandler : IEventHandler<MemberChangedEvent>, IEventHandler<UserChangedEvent>, IEventHandler<UserRoleAddedEvent>, IEventHandler<UserRoleRemovedEvent>
    {
        private readonly IChangeLogService _changeLogService;

        public LogChangesEventHandler(IChangeLogService changeLogService)
        {
            _changeLogService = changeLogService;
        }

        public virtual Task Handle(MemberChangedEvent @event)
        {
            InnerHandle(@event);
            return Task.CompletedTask;
        }

        public virtual Task Handle(UserChangedEvent message)
        {
            return InnerUserEventHandle(message);
        }

        public virtual Task Handle(UserRoleAddedEvent message)
        {
            return InnerUserEventHandle(message);
        }

        public virtual Task Handle(UserRoleRemovedEvent message)
        {
            return InnerUserEventHandle(message);
        }

        [DisableConcurrentExecution(10)]
        // "DisableConcurrentExecutionAttribute" prevents to start simultaneous job payloads.
        // Should have short timeout, because this attribute implemented by following manner: newly started job falls into "processing" state immediately.
        // Then it tries to receive job lock during timeout. If the lock received, the job starts payload.
        // When the job is awaiting desired timeout for lock release, it stucks in "processing" anyway. (Therefore, you should not to set long timeouts (like 24*60*60), this will cause a lot of stucked jobs and performance degradation.)
        // Then, if timeout is over and the lock NOT acquired, the job falls into "scheduled" state (this is default fail-retry scenario).
        // Failed job goes to "Failed" state (by default) after retries exhausted.
        public void LogEntityChangesInBackground(IList<OperationLog> operationLogs)
        {
            _changeLogService.SaveChangesAsync(operationLogs.ToArray()).GetAwaiter().GetResult();
        }


        protected virtual void InnerHandle<T>(GenericChangedEntryEvent<T> @event) where T : IEntity
        {
            // ObjectType has to be 'Member' as MemberDocumentChangesProvider uses it to get all changed members in 1 request.
            var logOperations = @event.ChangedEntries.Select(x => AbstractTypeFactory<OperationLog>.TryCreateInstance().FromChangedEntry(x, nameof(Member))).ToList();
            //Background task is used here for performance reasons
            BackgroundJob.Enqueue(() => LogEntityChangesInBackground(logOperations));
        }

        protected virtual Task InnerUserEventHandle(DomainEvent message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var operationLogs = new List<OperationLog>();

            switch (message)
            {
                case UserChangedEvent userChangedEvent:
                    operationLogs = userChangedEvent.ChangedEntries
                        .Where(x => !string.IsNullOrEmpty(x.OldEntry.MemberId))
                        .Select(x => GetOperationLog(x.OldEntry.MemberId))
                        .ToList();
                    break;
                case UserRoleAddedEvent userRoleAddedEvent when !string.IsNullOrEmpty(userRoleAddedEvent.User.MemberId):
                    operationLogs.Add(GetOperationLog(userRoleAddedEvent.User.MemberId));
                    break;
                case UserRoleRemovedEvent userRoleRemovedEvent when !string.IsNullOrEmpty(userRoleRemovedEvent.User.MemberId):
                    operationLogs.Add(GetOperationLog(userRoleRemovedEvent.User.MemberId));
                    break;
            }

            BackgroundJob.Enqueue(() => LogEntityChangesInBackground(operationLogs));

            return Task.CompletedTask;
        }

        protected virtual OperationLog GetOperationLog(string objectId)
        {
            var result = new OperationLog
            {
                ObjectId = objectId,
                ObjectType = nameof(Member),
                OperationType = EntryState.Modified,
            };

            return result;
        }
    }
}
