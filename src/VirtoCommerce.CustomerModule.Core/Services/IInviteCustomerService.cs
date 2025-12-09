using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Security;

namespace VirtoCommerce.CustomerModule.Core.Services;

public interface IInviteCustomerService
{
    Task<InviteCustomerResult> InviteCustomerAsyc(InviteCustomerRequest request, CancellationToken cancellationToken = default);

    Task<IList<Role>> GetInviteRolesAsync();
}
