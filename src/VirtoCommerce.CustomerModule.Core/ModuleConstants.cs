using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.CustomerModule.Core
{
    [ExcludeFromCodeCoverage]
    public static class ModuleConstants
    {
        public static class Security
        {
            public static class Permissions
            {
                public const string Read = "customer:read";
                public const string Create = "customer:create";
                public const string Access = "customer:access";
                public const string Update = "customer:update";
                public const string Delete = "customer:delete";

                public static string[] AllPermissions { get; } = { Read, Create, Access, Update, Delete };
            }
        }

        public static class Settings
        {
            public static class General
            {
                public static SettingDescriptor MemberGroups { get; } = new SettingDescriptor
                {
                    Name = "Customer.MemberGroups",
                    GroupName = "Customer|General",
                    ValueType = SettingValueType.ShortText,
                    IsDictionary = true,
                    DefaultValue = "New",
                    AllowedValues = new object[] { "VIP", "Wholesaler" }
                };

                public static SettingDescriptor ExportImportPageSize { get; } = new SettingDescriptor
                {
                    Name = "Customer.ExportImport.PageSize",
                    GroupName = "Customer|General",
                    ValueType = SettingValueType.PositiveInteger,
                    DefaultValue = 50
                };

                public static SettingDescriptor MemberIndexationDate { get; } = new SettingDescriptor
                {
                    Name = "VirtoCommerce.Search.IndexingJobs.IndexationDate.Member",
                    GroupName = "Customer|General",
                    ValueType = SettingValueType.DateTime,
                    DefaultValue = default(DateTime),
                };

                public static SettingDescriptor EventBasedIndexation { get; } = new SettingDescriptor
                {
                    Name = "Customer.Search.EventBasedIndexation.Enable",
                    GroupName = "Customer|General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false
                };

                #region Statuses

                public static SettingDescriptor OrganizationStatuses { get; } = new SettingDescriptor
                {
                    Name = "Customer.OrganizationStatuses",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Customer|Statuses",
                    IsDictionary = true,
                    DefaultValue = "New",
                    AllowedValues = new[] { "New", "Approved", "Rejected", "Deleted" }
                };

                public static SettingDescriptor VendorStatuses { get; } = new SettingDescriptor
                {
                    Name = "Customer.VendorStatuses",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Customer|Statuses",
                    IsDictionary = true,
                    DefaultValue = "New",
                    AllowedValues = new[] { "New", "Approved", "Rejected", "Deleted" }
                };

                public static SettingDescriptor EmployeeStatuses { get; } = new SettingDescriptor
                {
                    Name = "Customer.EmployeeStatuses",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Customer|Statuses",
                    IsDictionary = true,
                    DefaultValue = "New",
                    AllowedValues = new[] { "New", "Approved", "Rejected", "Deleted" }
                };

                public static SettingDescriptor ContactStatuses { get; } = new SettingDescriptor
                {
                    Name = "Customer.ContactStatuses",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Customer|Statuses",
                    IsDictionary = true,
                    DefaultValue = "New",
                    AllowedValues = new[] { "New", "Approved", "Rejected", "Deleted" }
                };

                public static SettingDescriptor OrganizationDefaultStatus { get; } = new SettingDescriptor
                {
                    Name = "Customer.OrganizationDefaultStatus",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Customer|Statuses",
                };

                public static SettingDescriptor ContactDefaultStatus { get; } = new SettingDescriptor
                {
                    Name = "Customer.ContactDefaultStatus",
                    ValueType = SettingValueType.ShortText,
                    GroupName = "Customer|Statuses",
                };

                #endregion Statuses

                public static IEnumerable<SettingDescriptor> AllSettings => new List<SettingDescriptor>
                {
                    MemberGroups,
                    ExportImportPageSize,
                    MemberIndexationDate,
                    EventBasedIndexation,
                    OrganizationStatuses,
                    VendorStatuses,
                    EmployeeStatuses,
                    ContactStatuses,
                    OrganizationDefaultStatus,
                    ContactDefaultStatus
                };
            }

            public static IEnumerable<SettingDescriptor> StoreLevelSettings
            {
                get
                {
                    yield return General.OrganizationDefaultStatus;
                    yield return General.ContactDefaultStatus;
                }
            }

            public static IEnumerable<SettingDescriptor> AllSettings => General.AllSettings;
        }
    }
}
