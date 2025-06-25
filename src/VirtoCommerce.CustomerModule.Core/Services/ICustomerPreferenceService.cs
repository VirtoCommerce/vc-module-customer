using System.Collections.Generic;
using System.Threading.Tasks;

namespace VirtoCommerce.CustomerModule.Core.Services;

public interface ICustomerPreferenceService
{
    Task<string> GetValue(string userId, IList<string> nameParts);
    Task<string> GetValue(string userId, string name);
    Task SaveValue(string userId, IList<string> nameParts, string value);
    Task SaveValue(string userId, string name, string value);
};
