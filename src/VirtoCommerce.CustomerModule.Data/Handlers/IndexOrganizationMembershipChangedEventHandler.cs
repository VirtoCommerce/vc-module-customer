using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.CustomerModule.Data.Search.Indexing;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.SearchModule.Core.BackgroundJobs;
using VirtoCommerce.SearchModule.Core.Extensions;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CustomerModule.Data.Handlers
{
    public class IndexOrganizationMembershipChangedEventHandler : IEventHandler<OrganizationMembershipChangedEvent>
    {
        private readonly Func<UserManager<ApplicationUser>> _userManagerFactory;
        private readonly IIndexingJobService _indexingJobService;
        private readonly IEnumerable<IndexDocumentConfiguration> _configurations;

        public IndexOrganizationMembershipChangedEventHandler(
            Func<UserManager<ApplicationUser>> userManagerFactory,
            IIndexingJobService indexingJobService,
            IEnumerable<IndexDocumentConfiguration> configurations)
        {
            _userManagerFactory = userManagerFactory;
            _indexingJobService = indexingJobService;
            _configurations = configurations;
        }

        public async Task Handle(OrganizationMembershipChangedEvent message)
        {
            var userIds = message.ChangedEntries
                .Select(x => x.NewEntry?.UserId ?? x.OldEntry?.UserId)
                .Where(id => !string.IsNullOrEmpty(id))
                .Distinct()
                .ToList();

            if (userIds.Count == 0)
            {
                return;
            }

            using var userManager = _userManagerFactory();

            var memberIds = new List<string>();
            foreach (var userId in userIds)
            {
                var user = await userManager.FindByIdAsync(userId);
                if (!string.IsNullOrEmpty(user?.MemberId))
                {
                    memberIds.Add(user.MemberId);
                }
            }

            if (memberIds.Count == 0)
            {
                return;
            }

            var indexEntries = memberIds
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
    }
}
