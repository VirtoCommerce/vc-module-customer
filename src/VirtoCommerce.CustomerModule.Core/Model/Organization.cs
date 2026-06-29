using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Core.Model
{
    public class Organization : Member
    {
        public Organization()
        {
            //Retain Organization as discriminator  in case of  derived types must have the same MemberType
            MemberType = nameof(Organization);
        }
        public string Description { get; set; }
        public string BusinessCategory { get; set; }
        public string OwnerId { get; set; }
        public string ParentId { get; set; }

        public IList<OrganizationRole> Roles { get; set; }

        public override string ObjectType => typeof(Organization).FullName;

        public override void ReduceDetails(string responseGroup)
        {
            base.ReduceDetails(responseGroup);

            var memberResponseGroup = EnumUtility.SafeParseFlags(responseGroup, MemberResponseGroup.Full);
            if (!memberResponseGroup.HasFlag(MemberResponseGroup.WithRoles))
            {
                Roles = null;
            }
        }
    }
}
