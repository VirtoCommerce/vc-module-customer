using VirtoCommerce.Platform.Core.ChangeLog;

namespace VirtoCommerce.CustomerModule.Data.Handlers
{
    /// <summary>
    /// Payload of the background job that persists member and user change-log entries.
    /// </summary>
    public class LogEntityChangesJobPayload
    {
        public OperationLog[] OperationLogs { get; set; }
    }
}
