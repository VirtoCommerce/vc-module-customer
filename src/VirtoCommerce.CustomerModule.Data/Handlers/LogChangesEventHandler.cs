using System;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
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

        /// <summary>
        /// Kept only so Hangfire jobs enqueued by an earlier version still resolve: the store persists this
        /// type and method by name, so deleting it would strand every queued entry as permanently Failed.
        /// New work goes through <see cref="LogEntityChangesJobHandler"/>; remove this once no store can
        /// still hold a job that predates the migration.
        /// </summary>
        [Obsolete("Enqueued indirectly by legacy Hangfire jobs only; new work uses LogEntityChangesJobHandler.", DiagnosticId = "VC0012", UrlFormat = "https://docs.virtocommerce.org/platform/user-guide/versions/virto3-products-versions/")]
        public Task LogEntityChangesInBackground(OperationLog[] operationLogs)
        {
            return _changeLogService.SaveChangesAsync(operationLogs);
        }

        public virtual Task Handle(MemberChangedEvent message)
        {
            // ObjectType has to be 'Member' as MemberDocumentChangesProvider uses it to get all changed members in 1 request.
            var logOperations = message.ChangedEntries
                .Select(x => AbstractTypeFactory<OperationLog>.TryCreateInstance().FromChangedEntry(x, nameof(Member)))
                .ToArray();

            return InnerHandle(logOperations);
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
            if (string.IsNullOrEmpty(message.User.MemberId))
            {
                return Task.CompletedTask;
            }

            return InnerHandle(GetOperationLog(message.User.MemberId));
        }

        public virtual Task Handle(UserRoleRemovedEvent message)
        {
            if (string.IsNullOrEmpty(message.User.MemberId))
            {
                return Task.CompletedTask;
            }

            return InnerHandle(GetOperationLog(message.User.MemberId));
        }

        protected virtual Task InnerHandle(params OperationLog[] operationLogs)
        {
            var payload = AbstractTypeFactory<LogEntityChangesJobPayload>.TryCreateInstance();
            payload.OperationLogs = operationLogs;

            // The static facade, not an injected IBackgroundJob: this handler is resolved once from the ROOT
            // provider by RegisterEventHandler and held for the process lifetime, so it cannot hold the Scoped
            // IBackgroundJob. The facade opens a short-lived scope per call, which is what it exists for.
            return BackgroundJob.Enqueue<LogEntityChangesJobHandler>(payload);
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
