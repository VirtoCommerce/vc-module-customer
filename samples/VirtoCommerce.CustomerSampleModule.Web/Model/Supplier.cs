using VirtoCommerce.CustomerModule.Core.Model;

namespace VirtoCommerce.CustomerSampleModule.Web.Model
{
    public class Supplier : Member
    {
        public string ContractNumber { get; set; }

        #region IHasDynamicProperties Members

        // Overriding the property to have own Dynamic Property type
        public override string ObjectType => typeof(Supplier).FullName;

        #endregion IHasDynamicProperties Members
    }
}
