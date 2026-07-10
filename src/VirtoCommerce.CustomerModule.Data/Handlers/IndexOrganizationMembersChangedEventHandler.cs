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
                .Where(x => x.EntryState == EntryState.Modified &&
                            x.NewEntry is Organization newOrg &&
                            x.OldEntry is Organization oldOrg &&
                            RolesChanged(oldOrg, newOrg))
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

        private static bool RolesChanged(Organization oldOrg, Organization newOrg)
        {
            // Null Roles on the saved model means the caller did not manage roles,
            // and the persistence layer leaves them intact (see OrganizationEntity.Patch)
            if (newOrg.Roles == null)
            {
                return false;
            }

            // Role names are indexed in member documents too, so compare both id and name
            var oldRoles = oldOrg.Roles?.Select(r => (r.RoleId, r.RoleName)).ToHashSet() ?? [];
            var newRoles = newOrg.Roles.Select(r => (r.RoleId, r.RoleName)).ToHashSet();

            return !oldRoles.SetEquals(newRoles);
        }

        private async Task<List<string>> GetMemberIdsAsync(IList<string> organizationIds)
        {
            var memberIds = new List<string>();

            foreach (var orgId in organizationIds)
            {
                var members = await _memberSearchService.SearchAllAsync(
                    new MembersSearchCriteria
                    {
                        MemberId = orgId
                    });

                memberIds.AddRange(members.Select(m => m.Id));
            }

            return memberIds;
        }
    }
}
