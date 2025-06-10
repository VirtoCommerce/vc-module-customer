using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.CustomerModule.Data.Services;

public class CustomerPreferenceCrudService(
    Func<ICustomerRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    IEventPublisher eventPublisher)
    : CrudService<CustomerPreference, CustomerPreferenceEntity, CustomerPreferenceChangingEvent, CustomerPreferenceChangedEvent>
        (repositoryFactory, platformMemoryCache, eventPublisher),
        ICustomerPreferenceCrudService
{
    protected override Task<IList<CustomerPreferenceEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((ICustomerRepository)repository).GetCustomerPreferencesByIdsAsync(ids, responseGroup);
    }
}
