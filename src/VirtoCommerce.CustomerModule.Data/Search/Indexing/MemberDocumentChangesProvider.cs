using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Data.Repositories;
using VirtoCommerce.Platform.Core.ChangeLog;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.SearchModule.Core.Model;
using VirtoCommerce.SearchModule.Core.Services;

namespace VirtoCommerce.CustomerModule.Data.Search.Indexing
{
    public class MemberDocumentChangesProvider : IIndexDocumentChangesProvider
    {
        public const string ChangeLogObjectType = nameof(Member);

        private readonly Func<IMemberRepository> _memberRepositoryFactory;
        private readonly IChangeLogSearchService _changeLogSearchService;

        public MemberDocumentChangesProvider(Func<IMemberRepository> memberRepositoryFactory, IChangeLogSearchService changeLogSearchService)
        {
            _memberRepositoryFactory = memberRepositoryFactory;
            _changeLogSearchService = changeLogSearchService;
        }

        public virtual async Task<long> GetTotalChangesCountAsync(DateTime? startDate, DateTime? endDate)
        {
            long result;

            if (startDate == null && endDate == null)
            {
                // Get total products count
                using (var repository = _memberRepositoryFactory())
                {
                    result = repository.Members.Count();
                }
            }
            else
            {
                var criteria = GetChangeLogSearchCriteria(startDate, endDate, 0, 0);
                // Get changes count from operation log
                result = (await _changeLogSearchService.SearchAsync(criteria)).TotalCount;
            }

            return result;
        }

        public virtual async Task<IList<IndexDocumentChange>> GetChangesAsync(DateTime? startDate, DateTime? endDate, long skip, long take)
        {
            IList<IndexDocumentChange> result;

            if (startDate == null && endDate == null)
            {
                // Get documents from repository and return them as changes
                using (var repository = _memberRepositoryFactory())
                {
                    var productIds = repository.Members
                        .OrderBy(i => i.CreatedDate)
                        .Select(i => i.Id)
                        .Skip((int)skip)
                        .Take((int)take)
                        .ToArray();

                    result = productIds.Select(id =>
                        new IndexDocumentChange
                        {
                            DocumentId = id,
                            ChangeType = IndexDocumentChangeType.Modified,
                            ChangeDate = DateTime.UtcNow
                        }
                    ).ToArray();
                }
            }
            else
            {
                var criteria = GetChangeLogSearchCriteria(startDate, endDate, skip, take);
                // Get changes from operation log
                var operations = (await _changeLogSearchService.SearchAsync(criteria)).Results;

                result = operations.Select(o =>
                    new IndexDocumentChange
                    {
                        DocumentId = o.ObjectId,
                        ChangeType = ConvertEntryStateToDocumentChangeType(o.OperationType),
                        ChangeDate = o.ModifiedDate ?? o.CreatedDate,
                    }
                ).ToArray();
            }

            return result;
        }

        private static IndexDocumentChangeType ConvertEntryStateToDocumentChangeType(EntryState entryState)
        {
            return entryState switch
            {
                EntryState.Added => IndexDocumentChangeType.Created,
                EntryState.Deleted => IndexDocumentChangeType.Deleted,
                _ => IndexDocumentChangeType.Modified,
            };
        }

        protected virtual ChangeLogSearchCriteria GetChangeLogSearchCriteria(DateTime? startDate, DateTime? endDate, long skip, long take)
        {
            var criteria = AbstractTypeFactory<ChangeLogSearchCriteria>.TryCreateInstance();

            var types = AbstractTypeFactory<Member>.AllTypeInfos.Select(x => x.TypeName).ToList();
            types.Add(nameof(Member));

            criteria.ObjectTypes = types;
            criteria.StartDate = startDate;
            criteria.EndDate = endDate;
            criteria.Skip = (int)skip;
            criteria.Take = (int)take;

            return criteria;
        }
    }
}
