using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.StoreModule.Core.Model;

namespace VirtoCommerce.CustomerModule.Data.Services;

public class CompanyMemberRoleService : ICompanyMemberRoleService
{
    private readonly ISettingsManager _settingsManager;

    public CompanyMemberRoleService(ISettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
    }

    public virtual async Task<IList<string>> GetAllowedRoleIdsAsync(string storeId)
    {
        var descriptor = ModuleConstants.Settings.General.MembershipRolesWhitelist;

        var setting = string.IsNullOrEmpty(storeId)
            ? null
            : await _settingsManager.GetObjectSettingAsync(descriptor.Name, nameof(Store), storeId);

        var allowedValues = setting?.Id != null ? setting.AllowedValues : descriptor.AllowedValues;

        return (allowedValues ?? [])
            .Select(x => x?.ToString())
            .Where(x => !x.IsNullOrEmpty())
            .ToList();
    }
}
