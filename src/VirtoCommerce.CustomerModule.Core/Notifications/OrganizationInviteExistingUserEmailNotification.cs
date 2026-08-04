using VirtoCommerce.NotificationsModule.Core.Types;

namespace VirtoCommerce.CustomerModule.Core.Notifications
{
    public class OrganizationInviteExistingUserEmailNotification : RegistrationInvitationNotificationBase
    {
        public OrganizationInviteExistingUserEmailNotification() : base(nameof(OrganizationInviteExistingUserEmailNotification))
        {
        }

        public string OrganizationName { get; set; }

        public string CustomerName { get; set; }
    }
}
