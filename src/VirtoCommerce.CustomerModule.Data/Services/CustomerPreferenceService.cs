using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.Services;

public class CustomerPreferenceService(
    ICustomerPreferenceCrudService crudService,
    ICustomerPreferenceSearchService searchService)
    : ICustomerPreferenceService
{
    public virtual Task<string> GetValue(string userId, IList<string> nameParts)
    {
        return GetValue(userId, GetName(nameParts));
    }

    public virtual async Task<string> GetValue(string userId, string name)
    {
        var preference = await GetPreference(userId, name);

        return preference?.Value;
    }

    public virtual Task SaveValue(string userId, IList<string> nameParts, string value)
    {
        return SaveValue(userId, GetName(nameParts), value);
    }

    public virtual async Task SaveValue(string userId, string name, string value)
    {
        var preference = await GetPreference(userId, name, create: true);
        preference.Value = value;

        await crudService.SaveChangesAsync([preference]);
    }


    protected virtual string GetName(IList<string> nameParts)
    {
        return string.Join(".", nameParts);
    }

    protected virtual async Task<CustomerPreference> GetPreference(string userId, string name, bool create = false)
    {
        var criteria = AbstractTypeFactory<CustomerPreferenceSearchCriteria>.TryCreateInstance();
        criteria.UserId = userId;
        criteria.Name = name;
        criteria.Take = 1;

        var preference = (await searchService.SearchAsync(criteria, clone: create)).Results.FirstOrDefault();

        if (preference is null && create)
        {
            preference = AbstractTypeFactory<CustomerPreference>.TryCreateInstance();
            preference.UserId = userId;
            preference.Name = name;
        }

        return preference;
    }
}
