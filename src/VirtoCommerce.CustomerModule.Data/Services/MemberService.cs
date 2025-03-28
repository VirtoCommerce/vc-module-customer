using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.Caching;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.Search;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Security.Caching;

namespace VirtoCommerce.CustomerModule.Data.Services
{
    /// <summary>
    /// Abstract base class for all derived custom members services used IMemberRepository for persistent
    /// </summary>
    public class MemberService : IMemberService
    {
        private readonly Func<IMemberRepository> _repositoryFactory;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPlatformMemoryCache _platformMemoryCache;
        private readonly IUserSearchService _userSearchService;
        private readonly AbstractValidator<Member> _memberValidator;
        private readonly ICountriesService _countriesService;

        public MemberService(
            Func<IMemberRepository> repositoryFactory,
            IUserSearchService userSearchService,
            IEventPublisher eventPublisher,
            IPlatformMemoryCache platformMemoryCache,
            AbstractValidator<Member> memberValidator,
            ICountriesService countriesService)
        {
            _repositoryFactory = repositoryFactory;
            _eventPublisher = eventPublisher;
            _platformMemoryCache = platformMemoryCache;
            _memberValidator = memberValidator;
            _countriesService = countriesService;
            _userSearchService = userSearchService;
        }

        #region IMemberService Members

        /// <summary>
        /// Return members by requested ids can be override for load extra data for resulting members
        /// </summary>
        public virtual Task<Member[]> GetByIdsAsync(string[] memberIds, string responseGroup = null, string[] memberTypes = null)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(GetByIdsAsync), string.Join("-", memberIds), responseGroup, memberTypes == null ? null : string.Join("-", memberTypes));
            return _platformMemoryCache.GetOrCreateExclusiveAsync(cacheKey, async cacheEntry =>
            {
                IList<Member> members;
                using (var repository = _repositoryFactory())
                {
                    repository.DisableChangesTracking();
                    //There is loading for all corresponding members conceptual model entities types
                    //query performance when TPT inheritance used it is too slow, for improve performance we are passing concrete member types in to the repository
                    var memberTypeInfos = AbstractTypeFactory<Member>.AllTypeInfos.Where(t => t.MappedType != null);
                    if (memberTypes != null)
                    {
                        var types = memberTypes;
                        memberTypeInfos = memberTypeInfos.Where(x => types.Any(x.IsAssignableTo));
                    }

                    memberTypes = memberTypeInfos.Select(t => t.MappedType.AssemblyQualifiedName).Distinct().ToArray();

                    var dataMembers = await repository.GetMembersByIdsAsync(memberIds, responseGroup, memberTypes);
                    members = ProcessModels(dataMembers, responseGroup);

                    ConfigureCache(cacheEntry, memberIds, dataMembers, members);
                }

                #region Load member security accounts by separate request

                if (!EnumUtility.SafeParseFlags(responseGroup, MemberResponseGroup.Full).HasFlag(MemberResponseGroup.WithSecurityAccounts))
                {
                    return members.ToArray();
                }

                var hasSecurityAccountMembers = members.OfType<IHasSecurityAccounts>().ToArray();
                if (hasSecurityAccountMembers.Length == 0)
                {
                    return members.ToArray();
                }

                var usersSearchResult = await _userSearchService.SearchUsersAsync(new UserSearchCriteria
                {
                    MemberIds = hasSecurityAccountMembers.Select(x => x.Id).ToList(),
                    Take = int.MaxValue,
                });

                foreach (var hasAccountMember in hasSecurityAccountMembers)
                {
                    hasAccountMember.SecurityAccounts = usersSearchResult.Results.Where(x => x.MemberId.EqualsIgnoreCase(hasAccountMember.Id)).ToList();

                    if (hasAccountMember.SecurityAccounts.Count > 0)
                    {
                        hasAccountMember.SecurityAccounts.ToList().ForEach(x => cacheEntry.AddExpirationToken(SecurityCacheRegion.CreateChangeTokenForUser(x)));
                    }
                }

                #endregion Load member security accounts by separate request

                return members.ToArray();
            });
        }

        public virtual async Task<Member> GetByIdAsync(string memberId, string responseGroup = null, string memberType = null)
        {
            if (string.IsNullOrEmpty(memberId))
            {
                return null;
            }

            var members = await GetByIdsAsync([memberId], responseGroup, !string.IsNullOrEmpty(memberType) ? [memberType] : null);
            return members.FirstOrDefault();
        }

        /// <summary>
        /// Create or update members in database
        /// </summary>
        /// <param name="members"></param>
        public virtual async Task SaveChangesAsync(Member[] members)
        {
            foreach (var member in members)
            {
                await _memberValidator.ValidateAndThrowAsync(member);
            }

            FillContactFullName(members);
            await FillAddressNames(members);

            var pkMap = new PrimaryKeyResolvingMap();
            var changedEntries = new List<GenericChangedEntry<Member>>();

            using var repository = _repositoryFactory();

            var existingMemberEntities = await repository.GetMembersByIdsAsync(members.Where(m => !m.IsTransient()).Select(m => m.Id).ToArray());

            foreach (var member in members)
            {
                var dataSourceMember = FromModel(member, pkMap);

                var dataTargetMember = existingMemberEntities.FirstOrDefault(m => m.Id == member.Id);
                if (dataTargetMember != null)
                {
                    // Workaround to trigger update of auditable fields when only updating navigation properties.
                    // Otherwise on update trigger is fired only when non navigation properties are updated.
                    dataTargetMember.ModifiedDate = DateTime.UtcNow;

                    // This extension is allow to get around breaking changes is introduced in EF Core 3.0 that leads to throw
                    // Database operation expected to affect 1 row(s) but actually affected 0 row(s) exception when trying to add the new children entities with manually set keys
                    // https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-3.0/breaking-changes#detectchanges-honors-store-generated-key-values
                    repository.TrackModifiedAsAddedForNewChildEntities(dataTargetMember);

                    if (!dataTargetMember.GetType().IsInstanceOfType(dataSourceMember))
                    {
                        throw new OperationCanceledException($"Unable to update an member with type {dataTargetMember.MemberType} by an member with type {dataSourceMember.MemberType} because they aren't in the inheritance hierarchy");
                    }
                    changedEntries.Add(new GenericChangedEntry<Member>(member, dataTargetMember.ToModel(AbstractTypeFactory<Member>.TryCreateInstance(member.MemberType)), EntryState.Modified));
                    dataSourceMember.Patch(dataTargetMember);
                }
                else
                {
                    repository.Add(dataSourceMember);
                    changedEntries.Add(new GenericChangedEntry<Member>(member, EntryState.Added));
                }
            }
            //Raise domain events
            await _eventPublisher.Publish(new MemberChangingEvent(changedEntries));
            await repository.UnitOfWork.CommitAsync();
            pkMap.ResolvePrimaryKeys();
            ClearCache(members);

            await _eventPublisher.Publish(new MemberChangedEvent(changedEntries));
        }

        public virtual async Task DeleteAsync(string[] ids, string[] memberTypes = null)
        {
            using var repository = _repositoryFactory();

            var members = await GetByIdsAsync(ids, null, memberTypes);
            if (members?.Length > 0)
            {
                var changedEntries = members.Select(x => new GenericChangedEntry<Member>(x, EntryState.Deleted)).ToArray();
                await _eventPublisher.Publish(new MemberChangingEvent(changedEntries));

                await repository.RemoveMembersByIdsAsync(members.Select(m => m.Id).ToArray());
                await repository.UnitOfWork.CommitAsync();
                ClearCache(members);

                await _eventPublisher.Publish(new MemberChangedEvent(changedEntries));
            }
        }

        protected virtual void ConfigureCache(MemoryCacheEntryOptions cacheOptions, string[] memberIds, IList<MemberEntity> entities, IList<Member> models)
        {
            //It is so important to generate change tokens for all ids even for not existing members to prevent an issue
            //with caching of empty results for non - existing objects that have the infinitive lifetime in the cache
            //and future unavailability to create objects with these ids.
            cacheOptions.AddExpirationToken(CustomerCacheRegion.CreateChangeToken(memberIds));

            var ancestorIds = entities.SelectMany(r => r.MemberRelations)
                .Where(x => !string.IsNullOrEmpty(x.AncestorId))
                .Select(x => x.AncestorId)
                .ToArray();

            var descendantIds = entities.SelectMany(x => x.MemberRelations)
                .Where(x => !string.IsNullOrEmpty(x.DescendantId))
                .Select(x => x.DescendantId)
                .ToArray();

            cacheOptions.AddExpirationToken(CustomerCacheRegion.CreateChangeToken(ancestorIds));
            cacheOptions.AddExpirationToken(CustomerCacheRegion.CreateChangeToken(descendantIds));
        }

        protected virtual void ClearCache(IEnumerable<Member> members)
        {
            var models = members as Member[] ?? members.ToArray();
            ClearSearchCache(models);

            foreach (var member in models.Where(x => !x.IsTransient()))
            {
                CustomerCacheRegion.ExpireMemberById(member.Id);
            }
        }

        protected virtual void ClearSearchCache(IList<Member> models)
        {
            CustomerSearchCacheRegion.ExpireRegion();
        }

        protected virtual IList<Member> ProcessModels(IList<MemberEntity> entities, string responseGroup)
        {
            return entities?
                .Select(x =>
                {
                    var model = ToModel(x);
                    return model is null ? null : ProcessModel(responseGroup, x, model);
                })
                .Where(x => x is not null)
                .ToArray();
        }

        protected virtual Member ToModel(MemberEntity entity)
        {
            var model = AbstractTypeFactory<Member>.TryCreateInstance(entity.MemberType);
            if (model is null)
            {
                return null;
            }

            entity.ToModel(model);
            return model;
        }

        /// <summary>
        /// Post-read processing of the model instance.
        /// A good place to make some additional actions, tune model data.
        /// Override to add some model data changes, calculations, etc...
        /// </summary>
        protected virtual Member ProcessModel(string responseGroup, MemberEntity entity, Member model)
        {
            model.ReduceDetails(responseGroup);
            return model;
        }

        protected virtual MemberEntity FromModel(Member model, PrimaryKeyResolvingMap keyMap)
        {
            var memberEntityType = AbstractTypeFactory<Member>.AllTypeInfos
                .Where(t => t.MappedType != null && t.IsAssignableTo(model.MemberType))
                .Select(t => t.MappedType)
                .FirstOrDefault()
                ?? throw new InvalidOperationException($"Cannot find entity type for member type: {model.MemberType}");

            var dataSourceMember = AbstractTypeFactory<MemberEntity>.TryCreateInstance(memberEntityType.Name)
                ?? throw new InvalidOperationException($"Cannot create instance of entity type: {memberEntityType.Name}");

            dataSourceMember.FromModel(model, keyMap);
            return dataSourceMember;
        }

        #endregion IMemberService Members

        protected virtual void FillContactFullName(Member[] members)
        {
            foreach (var member in members.OfType<IHasPersonName>())
            {
                if (string.IsNullOrWhiteSpace(member.FullName))
                {
                    member.FullName = $"{member.FirstName} {member.LastName}".Trim();
                }
            }
        }

        protected virtual async Task FillAddressNames(Member[] members)
        {
            foreach (var member in members.Where(x => !x.Addresses.IsNullOrEmpty()))
            {
                foreach (var address in member.Addresses)
                {
                    if (string.IsNullOrEmpty(address.CountryName))
                    {
                        address.CountryName = _countriesService.GetByCode(address.CountryCode).Name;
                    }

                    if (string.IsNullOrEmpty(address.RegionName))
                    {
                        var regions = await _countriesService.GetCountryRegionsAsync(address.CountryCode);
                        address.RegionName = regions.FirstOrDefault(x => x.Id == address.RegionId)?.Name;
                    }
                }
            }
        }
    }
}
