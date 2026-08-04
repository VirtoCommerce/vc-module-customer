using VirtoCommerce.NotificationsModule.Core.Types;

namespace VirtoCommerce.CustomerModule.Core.Notifications
{
    public class CustomerInviteNewUserEmailNotification : RegistrationInvitationNotificationBase
    {
        public CustomerInviteNewUserEmailNotification() : base(nameof(CustomerInviteNewUserEmailNotification))
        {
        }
    }
}
