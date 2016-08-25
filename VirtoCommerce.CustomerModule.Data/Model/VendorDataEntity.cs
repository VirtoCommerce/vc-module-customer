using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Omu.ValueInjecter;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Common.ConventionInjections;

namespace VirtoCommerce.CustomerModule.Data.Model
{
    public class VendorDataEntity : MemberDataEntity
    {
        public string Description { get; set; }
        [StringLength(2048)]
        public string SiteUrl { get; set; }
        [StringLength(2048)]
        public string LogoUrl { get; set; }
        [StringLength(64)]
        public string GroupName { get; set; }

        public override Member ToModel(Member member)
        {
            //Call base converter first
            base.ToModel(member);
            return member;
        }

        public override MemberDataEntity FromModel(Member member, PrimaryKeyResolvingMap pkMap)
        {
            //Call base converter
            return base.FromModel(member, pkMap);
        }

        public override void Patch(MemberDataEntity memberEntity)
        {
            var target = memberEntity as VendorDataEntity;

            target.SiteUrl = this.SiteUrl;
            target.LogoUrl = this.LogoUrl;
            target.GroupName = this.GroupName;
            target.Description = this.Description;
                   
            base.Patch(target);
        }
    }
}
