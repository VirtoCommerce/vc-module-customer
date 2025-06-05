using System.Collections.Generic;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Events;

namespace VirtoCommerce.CustomerModule.Core.Events;

public class CustomerPreferenceChangingEvent(IEnumerable<GenericChangedEntry<CustomerPreference>> changedEntries)
    : GenericChangedEntryEvent<CustomerPreference>(changedEntries);
