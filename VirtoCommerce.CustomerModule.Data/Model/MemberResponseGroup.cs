using System;

namespace VirtoCommerce.CustomerModule.Data.Model
{
    /// <summary>
    /// Represents a enum  is used for manage fullness of resulting object graph
    /// </summary>
    [Flags]
    public enum MemberResponseGroup
    {
        None = 0,
        WithAncestors = 1,
        WithNotes = 1 << 1,
        WithEmails = 1 << 2,
        WithAddresses = 1 << 3,
        WithPhones = 1 << 4,
        WithGroups = 1 << 5,
        WithSecurityAccounts = 1 << 6,
        WithDynamicProperties = 1 << 7,
        WithSeo = 1 << 8,

        Full = WithAncestors | WithNotes | WithEmails | WithAddresses | WithPhones | WithGroups | WithSecurityAccounts | WithDynamicProperties | WithSeo
    }
}
