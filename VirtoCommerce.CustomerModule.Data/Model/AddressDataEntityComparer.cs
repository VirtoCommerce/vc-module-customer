using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Data.Model;

namespace VirtoCommerce.CustomerModule.Data.Model
{
    public class AddressDataEntityComparer : IEqualityComparer<AddressDataEntity>
    {
        #region IEqualityComparer<Discount> Members

        public virtual bool Equals(AddressDataEntity x, AddressDataEntity y)
        {
            return x.Id == y.Id;
        }

        public virtual int GetHashCode(AddressDataEntity obj)
        {
            return obj.Id != null ? obj.Id.GetHashCode() : obj.GetHashCode();
        }

        #endregion
    }
}
