using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerSampleModule.Web.Model
{
    public class Contact2Entity : ContactEntity
    {
        [StringLength(128)]
        public string JobTitle { get; set; }

        [StringLength(128)]
        public string WebContactId { get; set; }

        public override MemberEntity FromModel(Member member, PrimaryKeyResolvingMap pkMap)
        {
            // Call base converter first
            base.FromModel(member, pkMap);

            if (member is Contact2 contact2)
            {
                JobTitle = contact2.JobTitle;
                WebContactId = contact2.WebContactId;
            }

            return this;
        }

        public override Member ToModel(Member member)
        {
            // Call base converter first
            base.ToModel(member);

            if (member is Contact2 contact2)
            {
                contact2.JobTitle = JobTitle;
                contact2.WebContactId = WebContactId;
            }

            return member;
        }

        public override void Patch(MemberEntity target)
        {
            base.Patch(target);

            if (target is Contact2Entity contact2)
            {
                contact2.JobTitle = JobTitle;
                contact2.WebContactId = WebContactId;
            }
        }
    }
}
