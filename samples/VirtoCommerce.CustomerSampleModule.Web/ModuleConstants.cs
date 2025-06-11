using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CustomerSampleModule.Web;

[ExcludeFromCodeCoverage]
public static class ModuleConstants
{
    public static class Settings
    {
        public static class General
        {
            public static SettingDescriptor WebContactIdTemplate = new SettingDescriptor
            {
                Name = "Customer.WebContactIdTemplate",
                ValueType = SettingValueType.ShortText,
                GroupName = "Customer|General",
                DefaultValue = "PP{0:yyMMdd}-{1:D5}"
            };


            public static IEnumerable<SettingDescriptor> AllSettings => [WebContactIdTemplate];
        }

        public static IEnumerable<SettingDescriptor> AllSettings => General.AllSettings;
    }
}
