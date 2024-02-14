using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.Caching;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.Services;

public class FavoriteAddressService : IFavoriteAddressService
{
    private readonly Func<IMemberRepository> _repositoryFactory;
    private readonly IPlatformMemoryCache _platformMemoryCache;

    public FavoriteAddressService(Func<IMemberRepository> repositoryFactory, IPlatformMemoryCache platformMemoryCache)
    {
        _repositoryFactory = repositoryFactory;
        _platformMemoryCache = platformMemoryCache;
    }

    public async Task AddAddressToFavoritesAsync(string userId, string addressId)
    {
        using var repository = _repositoryFactory();
        var entity = await LoadEntity(repository, userId, addressId);

        if (entity == null)
        {
            entity = AbstractTypeFactory<FavoriteAddressEntity>.TryCreateInstance();
            entity.UserId = userId;
            entity.AddressId = addressId;

            repository.Add(entity);
            await repository.UnitOfWork.CommitAsync();
        }

        ClearCache(userId);
    }

    public async Task RemoveAddressFromFavoritesAsync(string userId, string addressId)
    {
        using var repository = _repositoryFactory();
        var entity = await LoadEntity(repository, userId, addressId);

        if (entity != null)
        {
            repository.Remove(entity);
            await repository.UnitOfWork.CommitAsync();
        }

        ClearCache(userId);
    }

    public Task<IList<string>> GetFavoriteAddressIdsAsync(string userId)
    {
        var cacheKey = CacheKey.With(GetType(), nameof(GetFavoriteAddressIdsAsync), userId);
        return _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, cacheOptions =>
        {
            cacheOptions.AddExpirationToken(CreateCacheToken(userId));
            return GetFavoriteAddressIdsNoCacheAsync(userId);
        });
    }


    protected virtual Task<FavoriteAddressEntity> LoadEntity(IMemberRepository repository, string userId, string addressId)
    {
        return repository.FavoriteAddresses.FirstOrDefaultAsync(x => x.UserId == userId && x.AddressId == addressId);
    }

    protected virtual async Task<IList<string>> GetFavoriteAddressIdsNoCacheAsync(string userId)
    {
        using var repository = _repositoryFactory();

        var addressIds = await repository.FavoriteAddresses
            .Where(x => x.UserId == userId)
            .Select(x => x.AddressId)
            .ToListAsync();

        return addressIds;
    }

    protected virtual IChangeToken CreateCacheToken(string userId)
    {
        return FavoriteAddressCacheRegion.CreateChangeTokenForKey(userId);
    }

    protected virtual void ClearCache(string userId)
    {
        FavoriteAddressCacheRegion.ExpireTokenForKey(userId);
    }
}
