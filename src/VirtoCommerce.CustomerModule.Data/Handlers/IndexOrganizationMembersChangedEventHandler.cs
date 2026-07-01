using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.CustomerModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.SearchModule.Core.BackgroundJobs;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CustomerModule.Data.Handlers
{
    public class IndexOrganizationMembersChangedEventHandler : IEventHandler<MemberChangedEvent>
    {
        private readonly IMemberSearchService _memberSearchService;
        private readonly IIndexingJobService _indexingJobService;
        private readonly IEnumerable<IndexDocumentConfiguration> _configurations;

        public IndexOrganizationMembersChangedEventHandler(
            IMemberSearchService memberSearchService,
            IIndexingJobService indexingJobService,
            IEnumerable<IndexDocumentConfiguration> configurations)
        {
            _memberSearchService = memberSearchService;
            _indexingJobService = indexingJobService;
            _configurations = configurations;
        }

        public async Task Handle(MemberChangedEvent message)
        {
            var organizationIds = message.ChangedEntries
                .Where(x => x.EntryState == EntryState.Modified && x.NewEntry is Organization)
                .Select(x => x.NewEntry.Id)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            if (organizationIds.Count == 0)
            {
                return;
            }

            var memberIds = await GetMemberIdsAsync(organizationIds);

            if (memberIds.Count == 0)
            {
                return;
            }

            var indexEntries = memberIds
                .Distinct()
                .Select(memberId =>
                {
                    var entry = AbstractTypeFactory<IndexEntry>.TryCreateInstance();
                    entry.Id = memberId;
                    entry.EntryState = EntryState.Modified;
                    entry.Type = KnownDocumentTypes.Member;
                    return entry;
                })
                .ToArray();

            _indexingJobService.EnqueueIndexAndDeleteDocuments(indexEntries, JobPriority.Normal,
                _configurations.GetDocumentBuilders(KnownDocumentTypes.Member, typeof(MemberDocumentChangesProvider)).ToList());
        }

        private async Task<List<string>> GetMemberIdsAsync(IList<string> organizationIds)
        {
            var memberIds = new List<string>();

            foreach (var orgId in organizationIds)
            {
                const int pageSize = 50;
                var criteria = new MembersSearchCriteria
                {
                    MemberId = orgId,
                    Take = pageSize
                };

                int totalCount;

                do
                {
                    var searchResult = await _memberSearchService.SearchMembersAsync(criteria);
                    totalCount = searchResult.TotalCount;
                    memberIds.AddRange(searchResult.Results.Select(m => m.Id));
                    criteria.Skip += pageSize;
                }
                while (criteria.Skip < totalCount);
            }
            return memberIds;
        }
    }
}
