using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
            var result = base.GetByIds(memberIds, responseGroup, memberTypes);
            var memberRespGroup = EnumUtility.SafeParseFlags(responseGroup, MemberResponseGroup.Full);
            //Load member security accounts by separate request
            if (memberRespGroup.HasFlag(MemberResponseGroup.WithSecurityAccounts))
            {
                var hasSecurityAccountMembers = result.OfType<IHasSecurityAccounts>();
                if (hasSecurityAccountMembers.Any())
                {
                    var usersSearchResult = Task.Factory.StartNew(() => _securityService.SearchUsersAsync(new UserSearchRequest { MemberIds = hasSecurityAccountMembers.Select(x => ((Member)x).Id).ToArray(), TakeCount = int.MaxValue }), System.Threading.CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();

                    foreach (var hasAccountMember in hasSecurityAccountMembers)
                    {
                        hasAccountMember.SecurityAccounts.AddRange(usersSearchResult.Users.Where(x => x.MemberId.EqualsInvariant(((Member)hasAccountMember).Id)));
                    }
                }
            }
            return result;
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
