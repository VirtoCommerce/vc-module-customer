using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Core.Model;

public class AddressSearchCriteria : SearchCriteriaBase
{
    public string MemberId { get; set; }

    private IList<string> _membersIds;
    public IList<string> MembersIds
    {
        get
        {
            if (_membersIds == null && !string.IsNullOrEmpty(MemberId))
            {
                _membersIds = [MemberId];
            }
            return _membersIds;
        }
        set
        {
            _membersIds = value;
        }
    }
}
