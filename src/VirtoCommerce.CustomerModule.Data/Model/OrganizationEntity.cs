using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerModule.Data.Model
{
    public class OrganizationEntity : MemberEntity
    {
        public string Description { get; set; }

        [StringLength(64)]
        public string BusinessCategory { get; set; }

        [StringLength(128)]
        public string OwnerId { get; set; }

        public override Member ToModel(Member member)
        {
            // Call base converter first
            base.ToModel(member);

            if (member is Organization organization)
            {
                organization.Description = Description;
                organization.OwnerId = OwnerId;
                organization.BusinessCategory = BusinessCategory;
                if (MemberRelations.Any())
                {
                    organization.ParentId = MemberRelations
                        .FirstOrDefault(x => x.RelationType == RelationType.Membership.ToString())?
                        .AncestorId;
                }

            }

            return member;
        }

        public override MemberEntity FromModel(Member member, PrimaryKeyResolvingMap pkMap)
        {
            // Call base converter first
            base.FromModel(member, pkMap);

            if (member is Organization organization)
            {
                Description = organization.Description;
                OwnerId = organization.OwnerId;
                BusinessCategory = organization.BusinessCategory;

                if (organization.ParentId != null)
                {
                    MemberRelations = new ObservableCollection<MemberRelationEntity>();

                    var memberRelation = AbstractTypeFactory<MemberRelationEntity>.TryCreateInstance();
                    memberRelation.AncestorId = organization.ParentId;
                    memberRelation.DescendantId = organization.Id;
                    memberRelation.AncestorSequence = 1;
                    memberRelation.RelationType = RelationType.Membership.ToString();

                    MemberRelations.Add(memberRelation);
                }
            }

            return this;
        }

        public override void Patch(MemberEntity target)
        {
            base.Patch(target);

            if (target is OrganizationEntity organization)
            {
                organization.Name = Name;
                organization.Description = Description;
                organization.OwnerId = OwnerId;
                organization.BusinessCategory = BusinessCategory;
            }
        }
    }
}
