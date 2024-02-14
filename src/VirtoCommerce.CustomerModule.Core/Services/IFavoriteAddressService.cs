using System.Collections.Generic;
using System.Threading.Tasks;

namespace VirtoCommerce.CustomerModule.Core.Services;

public interface IFavoriteAddressService
{
    Task AddAddressToFavoritesAsync(string userId, string addressId);
    Task RemoveAddressFromFavoritesAsync(string userId, string addressId);
    Task<IList<string>> GetFavoriteAddressIdsAsync(string userId);
}
