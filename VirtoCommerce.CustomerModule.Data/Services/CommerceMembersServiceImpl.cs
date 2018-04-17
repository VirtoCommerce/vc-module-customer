using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.Domain.Commerce.Services;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerModule.Data.Services
{
    /// <summary>
    /// Members service support CRUD and search for Contact, Organization, Vendor and Employee member types
    /// </summary>
    public class CommerceMembersServiceImpl : MemberServiceBase
    {
        private readonly ISecurityService _securityService;
        public CommerceMembersServiceImpl(Func<ICustomerRepository> repositoryFactory, IDynamicPropertyService dynamicPropertyService, ICommerceService commerceService, ISecurityService securityService, IEventPublisher eventPublisher)
            : base(repositoryFactory, dynamicPropertyService, commerceService, eventPublisher)
        {
            _securityService = securityService;
        }


        #region IMemberService Members

        public override Member[] GetByIds(string[] memberIds, string responseGroup = null, string[] memberTypes = null)
        {
            var retVal = base.GetByIds(memberIds, responseGroup, memberTypes);
            Parallel.ForEach(retVal, new ParallelOptions { MaxDegreeOfParallelism = 10 }, member =>
            {
                //Load security accounts for members which support them 
                var hasSecurityAccounts = member as IHasSecurityAccounts;
                if (hasSecurityAccounts != null)
                {
                    //Load all security accounts associated with this contact
                    var result = Task.Run(() => _securityService.SearchUsersAsync(new UserSearchRequest { MemberId = member.Id, TakeCount = int.MaxValue })).Result;
                    hasSecurityAccounts.SecurityAccounts.AddRange(result.Users);
                }
            });
            return retVal;
        }
        #endregion

        [SuppressMessage("ReSharper", "TryCastAlwaysSucceeds")]
        protected override Expression<Func<MemberDataEntity, bool>> GetQueryPredicate(MembersSearchCriteria criteria)
        {
            var retVal = base.GetQueryPredicate(criteria);

            if (!string.IsNullOrEmpty(criteria.SearchPhrase))
            {
                //where x or (y1 or y2)
                var predicate = PredicateBuilder.False<MemberDataEntity>();
                //search in special properties
                // do NOT use explicit conversion (also called direct or unsafe) cast i.e. (T(x)). EF doesn't support that
                predicate = predicate.Or(x => x is ContactDataEntity && (x as ContactDataEntity).FullName.Contains(criteria.SearchPhrase));
                predicate = predicate.Or(x => x is EmployeeDataEntity && (x as EmployeeDataEntity).FullName.Contains(criteria.SearchPhrase));
                //Should use Expand() to all predicates to prevent EF error
                //http://stackoverflow.com/questions/2947820/c-sharp-predicatebuilder-entities-the-parameter-f-was-not-bound-in-the-specif?rq=1
                retVal = LinqKit.Extensions.Expand(retVal.Or(LinqKit.Extensions.Expand(predicate)));
            }

            return retVal;
        }
    }
}
