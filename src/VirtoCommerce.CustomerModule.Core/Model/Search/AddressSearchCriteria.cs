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

    public string RegionId { get; set; }

    private IList<string> _regionIds;
    public IList<string> RegionIds
    {
        get
        {
            if (_regionIds == null && !string.IsNullOrEmpty(RegionId))
            {
                _regionIds = [RegionId];
            }
            return _regionIds;
        }
        set
        {
            _regionIds = value;
        }
    }


    public string CountryCode { get; set; }

    private IList<string> _countryCodes;
    public IList<string> CountryCodes
    {
        get
        {
            if (_countryCodes == null && !string.IsNullOrEmpty(CountryCode))
            {
                _countryCodes = [CountryCode];
            }
            return _countryCodes;
        }
        set
        {
            _countryCodes = value;
        }
    }

    public string City { get; set; }

    private IList<string> _cities;

    public IList<string> Cities
    {
        get
        {
            if (_cities == null && !string.IsNullOrEmpty(City))
            {
                _cities = [City];
            }
            return _cities;
        }
        set
        {
            _cities = value;
        }
    }
}
