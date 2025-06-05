using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;

namespace VirtoCommerce.CustomerModule.Data.Services;

public class CustomerPreferenceSearchService(
    Func<ICustomerRepository> repositoryFactory,
    IPlatformMemoryCache platformMemoryCache,
    ICustomerPreferenceCrudService crudService,
    IOptions<CrudOptions> crudOptions)
    : SearchService<CustomerPreferenceSearchCriteria, CustomerPreferenceSearchResult, CustomerPreference, CustomerPreferenceEntity>
        (repositoryFactory, platformMemoryCache, crudService, crudOptions),
        ICustomerPreferenceSearchService
{
    protected override IQueryable<CustomerPreferenceEntity> BuildQuery(IRepository repository, CustomerPreferenceSearchCriteria criteria)
    {
        var query = ((ICustomerRepository)repository).CustomerPreferences;

        if (criteria.UserId != null)
        {
            query = query.Where(x => x.UserId == criteria.UserId);
        }

        if (criteria.Name != null)
        {
            query = query.Where(x => x.Name == criteria.Name);
        }

        if (!criteria.ObjectIds.IsNullOrEmpty())
        {
            query = criteria.ObjectIds.Count == 1
                ? query.Where(x => x.Id == criteria.ObjectIds[0])
                : query.Where(x => criteria.ObjectIds.Contains(x.Id));
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(CustomerPreferenceSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(CustomerPreferenceEntity.CreatedDate), SortDirection = SortDirection.Descending },
                new SortInfo { SortColumn = nameof(CustomerPreferenceEntity.Id) },
            ];
        }

        return sortInfos;
    }
}
