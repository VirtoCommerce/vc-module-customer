using System.ComponentModel.DataAnnotations;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CustomerSampleModule.Web.Model
{
    public class SupplierEntity : MemberEntity
    {
        [StringLength(128)]
        public string ContractNumber { get; set; }

        public override Member ToModel(Member member)
        {
            // Call base converter first
            base.ToModel(member);

            if (member is Supplier supplier)
            {
                supplier.ContractNumber = ContractNumber;
            }

            return member;
        }

        public override MemberEntity FromModel(Member member, PrimaryKeyResolvingMap pkMap)
        {
            // Call base converter first
            base.FromModel(member, pkMap);

            if (member is Supplier supplier)
            {
                ContractNumber = supplier.ContractNumber;
            }

            return this;
        }

        public override void Patch(MemberEntity target)
        {
            base.Patch(target);

            if (target is SupplierEntity supplier)
            {
                supplier.ContractNumber = ContractNumber;
            }
        }
    }
}
