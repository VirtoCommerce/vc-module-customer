using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Domain.Customer.Services;
using VirtoCommerce.Domain.Store.Services;
using VirtoCommerce.Platform.Core.Notifications;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Domain.Payment.Model;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Domain.Customer.Events;
using VirtoCommerce.Domain.Search;
using VirtoCommerce.CustomerModule.Data.Search.Indexing;

namespace VirtoCommerce.CustomerModule.Data.Observers
{
    public class MemberIndexationObserver : IObserver<MemberChangingEvent>
    {
        private readonly IStoreService _storeService;
        private readonly IIndexingManager _indexingManager;

        public MemberIndexationObserver(IStoreService storeService, IIndexingManager indexingManager)
        {
            _storeService = storeService;
            _indexingManager = indexingManager;
        }

        public void OnCompleted()
        {
            
        }

        public void OnError(Exception error)
        {
            
        }

        public void OnNext(MemberChangingEvent changeEvent)
        {
            if (changeEvent.ChangeState == EntryState.Added && changeEvent.Member.Id != null)
            {
                IndexingOptions indexingOptions = new IndexingOptions {
                    DocumentType = KnownDocumentTypes.Member,
                    DeleteExistingIndex = false,
                    DocumentIds = new List<string> { changeEvent.Member.Id.ToString() }
                };

                _indexingManager.IndexAsync(indexingOptions, null, new System.Threading.CancellationToken(false));
            }
        }

    }
}
