using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Data.Search;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Services;

namespace VirtoCommerce.CustomerModule.Data.Services
{
    public class MemberSearchServiceDecorator : IMemberSearchService
    {
        private readonly CommerceMembersServiceImpl _memberSearchService;
        private readonly MemberIndexedSearchService _memberIndexedSearchService;

        public MemberSearchServiceDecorator(CommerceMembersServiceImpl memberSearchService, MemberIndexedSearchService memberIndexedSearchService)
        {
            _memberSearchService = memberSearchService;
            _memberIndexedSearchService = memberIndexedSearchService;
        }

        public virtual GenericSearchResult<Member> SearchMembers(MembersSearchCriteria criteria)
        {
            var result = !string.IsNullOrEmpty(criteria?.SearchPhrase)
                ? SearchIndex(criteria)
                : _memberSearchService.SearchMembers(criteria);

            return result;
        }


        protected virtual GenericSearchResult<Member> SearchIndex(MembersSearchCriteria criteria)
        {
            return Task.Run(() => SearchIndexAsync(criteria)).GetAwaiter().GetResult();
        }

        protected virtual Task<GenericSearchResult<Member>> SearchIndexAsync(MembersSearchCriteria criteria)
        {
            return _memberIndexedSearchService.SearchAsync(criteria);
        }
    }
}
