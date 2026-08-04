using VirtoCommerce.NotificationsModule.Core.Types;

namespace VirtoCommerce.CustomerModule.Core.Notifications
{
    public class OrganizationInviteNewUserEmailNotification : RegistrationInvitationNotificationBase
    {
        public OrganizationInviteNewUserEmailNotification() : base(nameof(OrganizationInviteNewUserEmailNotification))
        {
        }

        public string OrganizationName { get; set; }
    }
}
