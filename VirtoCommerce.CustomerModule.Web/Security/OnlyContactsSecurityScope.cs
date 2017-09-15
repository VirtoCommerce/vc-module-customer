using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VirtoCommerce.Domain.Customer.Model;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerModule.Web.Security
{
    public class OnlyContactsSecurityScope : PermissionScope
    {
        public OnlyContactsSecurityScope()
        {
            Scope = "Contact";
        }
        public override bool IsScopeAvailableForPermission(string permission)
        {
            return permission == CustomerPredefinedPermissions.Update;
        }

        public override IEnumerable<string> GetEntityScopeStrings(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            var member = obj as Member;
        
            if (member != null)
            {
                return new[] { Type + ":" + obj.GetType().Name };
            }
            return Enumerable.Empty<string>();
        }
    }
}