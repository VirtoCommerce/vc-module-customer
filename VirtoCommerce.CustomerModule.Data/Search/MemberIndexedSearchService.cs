using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Model.Search;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Domain.Search;

namespace VirtoCommerce.CustomerModule.Data.Search
{
    public class MemberIndexedSearchService : IndexedSearchServiceBase<Member, MemberSearchCriteria>, IMemberIndexedSearchService
    {
        private readonly IMemberService _memberService;

        public MemberIndexedSearchService(ISearchRequestBuilder[] searchRequestBuilders, ISearchProvider searchProvider, IMemberService memberService)
            : base(searchRequestBuilders, searchProvider)
        {
            _memberService = memberService;
        }

        protected override IList<Member> GetItemsByIds(IList<string> itemIds, MemberSearchCriteria criteria)
        {
            var result = _memberService.GetByIds(itemIds.ToArray(), criteria.ResponseGroup, criteria.MemberTypes);
            return result;
        }
    }
}
