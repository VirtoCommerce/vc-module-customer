using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CustomerModule.Data.Search
{
    public class MemberSearchRequestBuilder : ISearchRequestBuilder
    {
        private readonly ISearchPhraseParser _searchPhraseParser;

        public MemberSearchRequestBuilder(ISearchPhraseParser searchPhraseParser)
        {
            _searchPhraseParser = searchPhraseParser;
        }

        public virtual string DocumentType { get; } = KnownDocumentTypes.Member;

        public virtual Task<SearchRequest> BuildRequestAsync(SearchCriteriaBase criteria)
        {
            SearchRequest request = null;

            var membersSearchCriteria = (criteria as MembersSearchCriteria)?.CloneTyped();
            if (membersSearchCriteria != null)
            {
                // Getting filters modifies search phrase
                var filters = GetFilters(membersSearchCriteria);

                request = new SearchRequest
                {
                    SearchKeywords = membersSearchCriteria.Keyword,
                    SearchFields = new[] { IndexDocumentExtensions.ContentFieldName },
                    Filter = filters.And(),
                    Sorting = GetSorting(membersSearchCriteria),
                    Skip = criteria.Skip,
                    Take = criteria.Take,
                };
            }

            return Task.FromResult(request);
        }


        protected virtual IList<IFilter> GetFilters(MembersSearchCriteria criteria)
        {
            var result = new List<IFilter>();

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                var parseResult = _searchPhraseParser.Parse(criteria.Keyword);
                criteria.Keyword = parseResult.Keyword;
                result.AddRange(parseResult.Filters);
            }

            if (criteria.ObjectIds?.Any() == true)
            {
                result.Add(new IdsFilter { Values = criteria.ObjectIds });
            }

            if (criteria.MemberTypes?.Any() == true)
            {
                result.Add(CreateTermFilter("MemberType", criteria.MemberTypes));
            }

            if (criteria.Groups?.Any() == true)
            {
                result.Add(CreateTermFilter("Groups", criteria.Groups));
            }

            if (!string.IsNullOrEmpty(criteria.MemberId))
            {
                result.Add(CreateTermFilter("ParentOrganizations", criteria.MemberId));
                // TODO: criteria.DeepSearch requires something like outlines in the catalog module
            }
            else if (!criteria.DeepSearch)
            {
                result.Add(CreateTermFilter("HasParentOrganizations", "false"));
            }

            return result;
        }

        protected virtual IList<SortingField> GetSorting(MembersSearchCriteria criteria)
        {
            var result = new List<SortingField>();

            foreach (var sortInfo in criteria.SortInfos)
            {
                var fieldName = sortInfo.SortColumn.ToLowerInvariant();
                var isDescending = sortInfo.SortDirection == SortDirection.Descending;
                result.Add(new SortingField(fieldName, isDescending));
            }

            return result;
        }

        protected static IFilter CreateTermFilter(string fieldName, string value)
        {
            return new TermFilter
            {
                FieldName = fieldName,
                Values = new[] { value },
            };
        }

        protected static IFilter CreateTermFilter(string fieldName, IEnumerable<string> values)
        {
            return new TermFilter
            {
                FieldName = fieldName,
                Values = values.ToArray(),
            };
        }
    }
}
