using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Core.Services.Indexed;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CustomerModule.Data.Search
{
    public class MemberIndexedSearchService : IIndexedMemberSearchService
    {
        private readonly ISearchRequestBuilderRegistrar _searchRequestBuilderRegistrar;
        private readonly ISearchProvider _searchProvider;
        private readonly IMemberService _memberService;

        public MemberIndexedSearchService(ISearchRequestBuilderRegistrar searchRequestBuilderRegistrar, ISearchProvider searchProvider, IMemberService memberService)
        {
            _searchRequestBuilderRegistrar = searchRequestBuilderRegistrar;
            _searchProvider = searchProvider;
            _memberService = memberService;
        }

        public virtual async Task<MemberSearchResult> SearchMembersAsync(MembersSearchCriteria criteria)
        {
            var requestBuilder = GetRequestBuilder(criteria);
            var request = await requestBuilder?.BuildRequestAsync(criteria);

            var response = await _searchProvider.SearchAsync(criteria.ObjectType, request);

            var result = await ConvertResponseAsync(response, criteria);
            return result;
        }

        protected virtual ISearchRequestBuilder GetRequestBuilder(MembersSearchCriteria criteria)
        {
            return _searchRequestBuilderRegistrar.GetRequestBuilderByDocumentType(criteria.ObjectType);
        }

        protected virtual async Task<MemberSearchResult> ConvertResponseAsync(SearchResponse response, MembersSearchCriteria criteria)
        {
            var result = AbstractTypeFactory<MemberSearchResult>.TryCreateInstance();

            if (response != null)
            {
                result.TotalCount = (int)response.TotalCount;
                result.Results = await ConvertDocumentsAsync(response.Documents, criteria);
            }

            return result;
        }

        protected virtual async Task<IList<Member>> ConvertDocumentsAsync(IList<SearchDocument> documents, MembersSearchCriteria criteria)
        {
            var result = new List<Member>();

            if (documents?.Any() == true)
            {
                var itemIds = documents.Select(doc => doc.Id).ToArray();
                var items = await GetMembersByIdsAsync(itemIds, criteria);
                var itemsMap = items.ToDictionary(m => m.Id, m => m);

                // Preserve documents order
                var members = documents
                    .Select(doc =>
                    {
                        var member = itemsMap.TryGetValue(doc.Id, out var value) ? value : null;

                        if (member != null)
                        {
                            member.RelevanceScore = doc.GetRelevanceScore();
                        }

                        return member;
                    })
                    .Where(m => m != null)
                    .ToArray();

                result.AddRange(members);
            }

            return result;
        }

        protected virtual async Task<IList<Member>> GetMembersByIdsAsync(IList<string> itemIds, MembersSearchCriteria criteria)
        {
            var result = await _memberService.GetByIdsAsync(itemIds.ToArray(), criteria.ResponseGroup, criteria.MemberTypes);
            return result;
        }
    }
}
