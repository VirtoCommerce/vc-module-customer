using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Commerce.Model.Search;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.Search
{
    public class MemberIndexedSearchService
    {
        private readonly ISearchRequestBuilder[] _searchRequestBuilders;
        private readonly ISearchProvider _searchProvider;
        private readonly IMemberService _memberService;

        public MemberIndexedSearchService(ISearchRequestBuilder[] searchRequestBuilders, ISearchProvider searchProvider, IMemberService memberService)
        {
            _searchRequestBuilders = searchRequestBuilders;
            _searchProvider = searchProvider;
            _memberService = memberService;
        }

        public virtual async Task<GenericSearchResult<Member>> SearchAsync(MembersSearchCriteria criteria)
        {
            var requestBuilder = GetRequestBuilder(criteria);
            var request = requestBuilder?.BuildRequest(criteria);

            var response = await _searchProvider.SearchAsync(criteria.ObjectType, request);

            var result = ConvertResponse(response, criteria);
            return result;
        }


        protected virtual ISearchRequestBuilder GetRequestBuilder(MembersSearchCriteria criteria)
        {
            var requestBuilder = _searchRequestBuilders?.FirstOrDefault(b => b.DocumentType.Equals(criteria.ObjectType)) ??
                                 _searchRequestBuilders?.FirstOrDefault(b => string.IsNullOrEmpty(b.DocumentType));

            if (requestBuilder == null)
                throw new InvalidOperationException($"No query builders found for document type '{criteria.ObjectType}'");

            return requestBuilder;
        }

        protected virtual GenericSearchResult<Member> ConvertResponse(SearchResponse response, MembersSearchCriteria criteria)
        {
            var result = AbstractTypeFactory<GenericSearchResult<Member>>.TryCreateInstance();

            if (response != null)
            {
                result.TotalCount = (int)response.TotalCount;
                result.Results = ConvertDocuments(response.Documents, criteria);
            }

            return result;
        }

        protected virtual ICollection<Member> ConvertDocuments(IList<SearchDocument> documents, MembersSearchCriteria criteria)
        {
            var result = new List<Member>();

            if (documents?.Any() == true)
            {
                var itemIds = documents.Select(doc => doc.Id).ToArray();
                var items = GeMembersByIds(itemIds, criteria);
                var itemsMap = items.ToDictionary(m => m.Id, m => m);

                // Preserve documents order
                var members = documents
                    .Select(doc => itemsMap.ContainsKey(doc.Id) ? itemsMap[doc.Id] : null)
                    .Where(m => m != null)
                    .ToArray();

                result.AddRange(members);
            }

            return result;
        }

        protected virtual IList<Member> GeMembersByIds(IList<string> itemIds, MembersSearchCriteria criteria)
        {
            var result = _memberService.GetByIds(itemIds.ToArray(), criteria.ResponseGroup, criteria.MemberTypes);
            return result;
        }
    }
}
