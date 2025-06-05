using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Services;

namespace VirtoCommerce.CustomerModule.Core.Extensions;

public static class CustomerPreferenceServiceExtensions
{
    public const string SelectedAddressId = "SelectedAddressId";

    public static Task<string> GetSelectedAddressId(this ICustomerPreferenceService service, string userId, string organizationId)
    {
        return service.GetValue(userId, [SelectedAddressId, organizationId]);
    }

    public static Task SaveSelectedAddressId(this ICustomerPreferenceService service, string userId, string organizationId, string addressId)
    {
        return service.SaveValue(userId, [SelectedAddressId, organizationId], addressId);
    }
}
