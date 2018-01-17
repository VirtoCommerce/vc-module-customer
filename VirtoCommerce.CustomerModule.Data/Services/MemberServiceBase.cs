using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Domain.Customer.Events;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.CustomerModule.Data.Services
{
    /// <summary>
    /// Abstract base class for all derived custom members services used IMemberRepository for persistent
    /// </summary>
    public abstract class MemberServiceBase : ServiceBase, IMemberService, IMemberSearchService
    {
        protected MemberServiceBase(Func<IMemberRepository> repositoryFactory, IDynamicPropertyService dynamicPropertyService, ICommerceService commerceService,
                                  IEventPublisher<MemberChangingEvent> eventPublisher)
        {
            RepositoryFactory = repositoryFactory;
            DynamicPropertyService = dynamicPropertyService;
            MemberEventventPublisher = eventPublisher;
            CommerceService = commerceService;
        }

        protected Func<IMemberRepository> RepositoryFactory { get; }
        protected IDynamicPropertyService DynamicPropertyService { get; }
        protected IEventPublisher<MemberChangingEvent> MemberEventventPublisher { get; }
        protected ICommerceService CommerceService { get; set; }


        #region IMemberService Members
        /// <summary>
        /// Return members by requested ids can be override for load extra data for resulting members
        /// </summary>
        /// <param name="memberIds"></param>
        /// <param name="responseGroup"></param>
        /// <param name="memberTypes"></param>
        /// <returns></returns>
        public virtual Member[] GetByIds(string[] memberIds, string responseGroup = null, string[] memberTypes = null)
        {
            var retVal = new List<Member>();
            using (var repository = RepositoryFactory())
            {
                repository.DisableChangesTracking();
                //There is loading for all coresponding members conseptual model entities types
                //query performance when TPT inheritance used it is too slow, for improve performance we are passing concrete member types in to the repository
                var memberTypeInfos = AbstractTypeFactory<Member>.AllTypeInfos.Where(t => t.MappedType != null);
                if (memberTypes != null)
                {
                    memberTypeInfos = memberTypeInfos.Where(x => memberTypes.Any(mt => x.IsAssignableTo(mt)));
                }
                memberTypes = memberTypeInfos.Select(t => t.MappedType.AssemblyQualifiedName).Distinct().ToArray();

                var dataMembers = repository.GetMembersByIds(memberIds, responseGroup, memberTypes);
                foreach (var dataMember in dataMembers)
                {
                    var member = AbstractTypeFactory<Member>.TryCreateInstance(dataMember.MemberType);
                    if (member != null)
                    {
                        dataMember.ToModel(member);
                        retVal.Add(member);
                    }
                }
            }

            //Load dynamic properties for member
            DynamicPropertyService.LoadDynamicPropertyValues(retVal.ToArray<IHasDynamicProperties>());

            CommerceService.LoadSeoForObjects(retVal.OfType<ISeoSupport>().ToArray());
            return retVal.ToArray();
        }

        /// <summary>
        /// Create or update members in database
        /// </summary>
        /// <param name="members"></param>
        public virtual void SaveChanges(Member[] members)
        {
            var pkMap = new PrimaryKeyResolvingMap();

            using (var repository = RepositoryFactory())
            using (var changeTracker = GetChangeTracker(repository))
            {
                var existingMemberEntities = repository.GetMembersByIds(members.Where(m => !m.IsTransient()).Select(m => m.Id).ToArray());

                foreach (var member in members)
                {
                    var memberEntityType = AbstractTypeFactory<Member>.AllTypeInfos.Where(t => t.MappedType != null && t.IsAssignableTo(member.MemberType)).Select(t => t.MappedType).FirstOrDefault();
                    if (memberEntityType != null)
                    {
                        var dataSourceMember = AbstractTypeFactory<MemberDataEntity>.TryCreateInstance(memberEntityType.Name);
                        if (dataSourceMember != null)
                        {
                            dataSourceMember.FromModel(member, pkMap);

                            var dataTargetMember = existingMemberEntities.FirstOrDefault(m => m.Id == member.Id);
                            if (dataTargetMember != null)
                            {
                                changeTracker.Attach(dataTargetMember);
                                dataSourceMember.Patch(dataTargetMember);
                                MemberEventventPublisher.Publish(new MemberChangingEvent(EntryState.Modified, member));
                            }
                            else
                            {
                                repository.Add(dataSourceMember);
                                MemberEventventPublisher.Publish(new MemberChangingEvent(EntryState.Added, member));
                            }
                        }
                    }
                }

                CommitChanges(repository);
                pkMap.ResolvePrimaryKeys();
            }

            //Save dynamic properties
            foreach (var member in members)
            {
                DynamicPropertyService.SaveDynamicPropertyValues(member);
            }

            CommerceService.UpsertSeoForObjects(members.OfType<ISeoSupport>().ToArray());
        }

        public virtual void Delete(string[] ids, string[] memberTypes = null)
        {
            using (var repository = RepositoryFactory())
            {
                var members = GetByIds(ids, null, memberTypes);
                if (!members.IsNullOrEmpty())
                {
                    foreach (var member in members)
                    {
                        MemberEventventPublisher.Publish(new MemberChangingEvent(EntryState.Deleted, member));
                    }
                    repository.RemoveMembersByIds(members.Select(m => m.Id).ToArray());
                    CommitChanges(repository);
                    foreach (var member in members)
                    {
                        DynamicPropertyService.DeleteDynamicPropertyValues(member);
                        var seoObject = member as ISeoSupport;
                        if (seoObject != null)
                        {
                            CommerceService.DeleteSeoForObject(seoObject);
                        }
                    }
                }
            }
        }
        #endregion

        #region IMemberSearchService Members
        /// <summary>
        /// Search members in database by given criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public virtual GenericSearchResult<Member> SearchMembers(MembersSearchCriteria criteria)
        {
            using (var repository = RepositoryFactory())
            {
                repository.DisableChangesTracking();

                var query = LinqKit.Extensions.AsExpandable(repository.Members);

                if (!criteria.MemberTypes.IsNullOrEmpty())
                {
                    query = query.Where(m => criteria.MemberTypes.Contains(m.MemberType));
                }

                if (!criteria.Groups.IsNullOrEmpty())
                {
                    query = query.Where(m => m.Groups.Any(g => criteria.Groups.Contains(g.Group)));
                }

                if (criteria.MemberId != null)
                {
                    //TODO: DeepSearch in specified member
                    query = query.Where(m => m.MemberRelations.Any(r => r.AncestorId == criteria.MemberId));
                }
                else if (!criteria.DeepSearch)
                {
                    query = query.Where(m => !m.MemberRelations.Any());
                }

                //Get extra predicates (where clause)
                var predicate = GetQueryPredicate(criteria);
                query = query.Where(LinqKit.Extensions.Expand(predicate));

                var sortInfos = criteria.SortInfos;
                if (sortInfos.IsNullOrEmpty())
                {
                    sortInfos = new[] {
                        new SortInfo { SortColumn = ReflectionUtility.GetPropertyName<Member>(m => m.MemberType), SortDirection = SortDirection.Descending },
                        new SortInfo { SortColumn = ReflectionUtility.GetPropertyName<Member>(m => m.Name), SortDirection = SortDirection.Ascending },
                    };
                }

                query = query.OrderBySortInfos(sortInfos);

                var totalCount = query.Count();
                var memberIds = query.Select(m => m.Id).Skip(criteria.Skip).Take(criteria.Take).ToList();

                var result = new GenericSearchResult<Member>
                {
                    TotalCount = totalCount,
                    Results = GetByIds(memberIds.ToArray(), criteria.ResponseGroup, criteria.MemberTypes)
                        .OrderBy(m => memberIds.IndexOf(m.Id))
                        .ToList(),
                };

                return result;
            }
        }
        #endregion

        /// <summary>
        /// Used to define extra where clause for members search
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        protected virtual Expression<Func<MemberDataEntity, bool>> GetQueryPredicate(MembersSearchCriteria criteria)
        {
            if (!string.IsNullOrEmpty(criteria.SearchPhrase))
            {
                var predicate = PredicateBuilder.False<MemberDataEntity>();
                predicate = predicate.Or(m => m.Name.Contains(criteria.SearchPhrase) || m.Emails.Any(e => e.Address.Contains(criteria.SearchPhrase)));
                //Should use Expand() to all predicates to prevent EF error
                //http://stackoverflow.com/questions/2947820/c-sharp-predicatebuilder-entities-the-parameter-f-was-not-bound-in-the-specif?rq=1
                return LinqKit.Extensions.Expand(predicate);
            }
            return PredicateBuilder.True<MemberDataEntity>();
        }
    }
}
