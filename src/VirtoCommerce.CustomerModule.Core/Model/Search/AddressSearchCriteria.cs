using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Core.Model;

public class AddressSearchCriteria : SearchCriteriaBase
{
    private string _memberId;
    public string MemberId
    {
        get => _memberId;
        set
        {
            if (_memberId != value)
            {
                _memberId = value;
                _membersIds = null;
            }
        }
    }

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

    private string _regionId;
    public string RegionId
    {
        get => _regionId;
        set
        {
            if (_regionId != value)
            {
                _regionId = value;
                _regionIds = null;
            }
        }
    }

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


    private string _countryCode;
    public string CountryCode
    {
        get => _countryCode;
        set
        {
            if (_countryCode != value)
            {
                _countryCode = value;
                _countryCodes = null;
            }
        }
    }

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

    private string _city;
    public string City
    {
        get => _city;
        set
        {
            if (_city != value)
            {
                _city = value;
                _cities = null;
            }
        }
    }

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
