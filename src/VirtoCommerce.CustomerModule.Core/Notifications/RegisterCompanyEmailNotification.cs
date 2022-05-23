using VirtoCommerce.NotificationsModule.Core.Model;

namespace VirtoCommerce.CustomerModule.Core.Notifications
{
    public class RegisterCompanyEmailNotification : EmailNotification
    {
        public RegisterCompanyEmailNotification() : base(nameof(RegisterCompanyEmailNotification))
        {

        }

        public string CompanyName { get; set; }
    }
}
