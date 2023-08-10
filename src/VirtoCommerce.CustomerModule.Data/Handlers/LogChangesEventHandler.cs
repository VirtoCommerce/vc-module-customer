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
            var operationLogs = message.ChangedEntries
                .Where(x => !string.IsNullOrEmpty(x.OldEntry.MemberId))
                .Select(x => GetOperationLog(x.OldEntry.MemberId))
                .ToArray();

            return InnerHandle(operationLogs);
        }

        public virtual Task Handle(UserRoleAddedEvent message)
        {
            return !string.IsNullOrEmpty(message.User.MemberId)
                ? InnerHandle(GetOperationLog(message.User.MemberId))
                : Task.CompletedTask;
        }

        public virtual Task Handle(UserRoleRemovedEvent message)
        {
            return !string.IsNullOrEmpty(message.User.MemberId)
                ? InnerHandle(GetOperationLog(message.User.MemberId))
                : Task.CompletedTask;
        }

        public void LogEntityChangesInBackground(IList<OperationLog> operationLogs)
        {
            _changeLogService.SaveChangesAsync(operationLogs.ToArray()).GetAwaiter().GetResult();
        }


        protected virtual void InnerHandle<T>(GenericChangedEntryEvent<T> @event) where T : IEntity
        {
            // ObjectType has to be 'Member' as MemberDocumentChangesProvider uses it to get all changed members in 1 request.
            var logOperations = @event.ChangedEntries.Select(x => AbstractTypeFactory<OperationLog>.TryCreateInstance().FromChangedEntry(x, nameof(Member))).ToArray();
            //Background task is used here for performance reasons
            BackgroundJob.Enqueue(() => LogEntityChangesInBackground(logOperations));
        }

        protected virtual Task InnerHandle(params OperationLog[] operationLogs)
        {
            BackgroundJob.Enqueue(() => LogEntityChangesInBackground(operationLogs));

            return Task.CompletedTask;
        }

        protected virtual OperationLog GetOperationLog(string memberId)
        {
            var result = AbstractTypeFactory<OperationLog>.TryCreateInstance();

            result.ObjectId = memberId;
            result.ObjectType = nameof(Member);
            result.OperationType = EntryState.Modified;

            return result;
        }
    }
}
