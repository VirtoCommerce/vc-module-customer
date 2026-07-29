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
        /// Kept for background jobs enqueued by an earlier version, which reference this method by name.
        /// New work goes through <see cref="LogEntityChangesJobHandler"/>; remove this once no such job
        /// can still be pending.
        /// </summary>
        [Obsolete("Enqueued indirectly by legacy Hangfire jobs only; new work uses LogEntityChangesJobHandler.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
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
            if (operationLogs.Length == 0)
            {
                // An empty batch is not free: SaveChangesAsync ends in Reset(), which expires the whole
                // change-log cache region, so enqueuing one would evict the cache for nothing.
                return Task.CompletedTask;
            }

            var payload = AbstractTypeFactory<LogEntityChangesJobPayload>.TryCreateInstance();
            payload.OperationLogs = operationLogs;

            // The static facade, not an injected IBackgroundJob: this handler is registered once from the root
            // provider and held for the process lifetime, so it must not capture a Scoped dependency.
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
