using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Core.Model.Search
{
    /// <summary>
    /// Represents common member search criteria. More specialized criteria should be derived for this type.
    /// </summary>
    public class MembersSearchCriteria : SearchCriteriaBase
    {
        public MembersSearchCriteria()
        {
            //this default value is required for  indexed search
            ObjectType = nameof(Member);
        }
        /// <summary>
        /// Search member type (Contact, Organization etc)
        /// </summary>
        public string MemberType { get; set; }

        private string[] _memberTypes;
        public string[] MemberTypes
        {
            get
            {
                if (_memberTypes == null && !string.IsNullOrEmpty(MemberType))
                {
                    _memberTypes = new[] { MemberType };
                }
                return _memberTypes;
            }
            set
            {
                _memberTypes = value;
            }
        }

        /// <summary>
        /// Search by member Groups  (VIP, Partner etc)
        /// </summary>
        public string Group { get; set; }

        private string[] _groups;
        public string[] Groups
        {
            get
            {
                if (_groups == null && !string.IsNullOrEmpty(Group))
                {
                    _groups = new[] { Group };
                }
                return _groups;
            }
            set
            {
                _groups = value;
            }
        }

        /// <summary>
        /// Search for child members of the given member (members of an organization, for example)
        /// </summary>
        public string MemberId { get; set; }

        private string[] _memberIds;
        /// <summary>
        /// Search for child members of any of the given members (plural counterpart of <see cref="MemberId"/>)
        /// </summary>
        public string[] MemberIds
        {
            get
            {
                if (_memberIds == null && !string.IsNullOrEmpty(MemberId))
                {
                    _memberIds = new[] { MemberId };
                }
                return _memberIds;
            }
            set
            {
                _memberIds = value;
            }
        }

        /// <summary>
        /// Deep search for child members of the given MemberId(s) or everything if no member is specified
        /// </summary>
        public bool DeepSearch { get; set; }

        /// <summary>
        /// Explicitly restrict the search to root members (members without a parent/membership relation), independently of MemberId(s).
        /// When <c>null</c> (default), the legacy behavior applies: root members only when no MemberId(s) is specified and <see cref="DeepSearch"/> is false.
        /// </summary>
        public bool? RootMembersOnly { get; set; }

        /// <summary>
        /// Search members by outerIds
        /// </summary>
        public string[] OuterIds { get; set; }

        /// <summary>
        /// Exclude members with the given ids from the search result.
        /// Applied before pagination, so page sizes and the total count stay consistent.
        /// </summary>
        public string[] ExcludedObjectIds { get; set; }


    }
}
