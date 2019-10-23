using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Core.Model.Search
{
    public class VendorSearchResult : GenericSearchResult<Vendor>
    {
        [Obsolete("Will be removed in future versions. Use Results instead")]
        public IList<Vendor> Vendors => Results;
    }
}
