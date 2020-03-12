using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Data.Common.ConventionInjections;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Platform.Core.Common;
using System.Collections.ObjectModel;

namespace VirtoCommerce.CustomerModule.Data.Model
{
    public class OrganizationDataEntity : MemberDataEntity
    {
        public int OrgType { get; set; }

        [StringLength(256)]
        public string Description { get; set; }

        [StringLength(64)]
        public string BusinessCategory { get; set; }

        [StringLength(128)]
        public string OwnerId { get; set; }

        public override Member ToModel(Member member)
        {
            //Call base converter first
            base.ToModel(member);

            var organization = member as Organization;
            if (organization != null && MemberRelations.Any())
            {
                organization.ParentId = MemberRelations.FirstOrDefault().AncestorId;
            }
            return member;
        }

        public override MemberDataEntity FromModel(Member member, PrimaryKeyResolvingMap pkMap)
        {
            var organization = member as Organization;

            if (organization != null && organization.ParentId != null)
            {
                MemberRelations = new ObservableCollection<MemberRelationDataEntity>();
                var memberRelation = new MemberRelationDataEntity
                {
                    AncestorId = organization.ParentId,
                    DescendantId = organization.Id,
                    AncestorSequence = 1
                };
                MemberRelations.Add(memberRelation);
            }

            //Call base converter
            return base.FromModel(member, pkMap);
        }

        public override void Patch(MemberDataEntity target)
        {
            if (target is OrganizationDataEntity targetOrganization)
            {
                targetOrganization.Name = Name;
                targetOrganization.Description = Description;
                targetOrganization.OwnerId = OwnerId;
                targetOrganization.OrgType = OrgType;
                targetOrganization.BusinessCategory = BusinessCategory;
            }

            base.Patch(target);
        }
    }
}
