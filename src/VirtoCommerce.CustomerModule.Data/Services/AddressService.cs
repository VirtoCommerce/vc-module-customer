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

public class AddressService(
    Func<IMemberRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    IEventPublisher eventPublisher)
    : CrudService<Address, AddressEntity, AddressChangingEvent, AddressChangedEvent>
        (repositoryFactory, platformMemoryCache, eventPublisher),
        IAddressService
{
    protected override Task<IList<AddressEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((IMemberRepository)repository).GetAddresssByIdsAsync(ids, responseGroup);
    }

    public override Task SaveChangesAsync(IList<Address> models)
    {
        throw new NotImplementedException("This service is read-only. Save addresses via the Member entity.");
    }

    public override Task DeleteAsync(IList<string> ids, bool softDelete = false)
    {
        throw new NotImplementedException("This service is read-only. Delete addresses via the Member entity.");
    }
}
