using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;

namespace VirtoCommerce.CustomerModule.Core.Services;

public interface IInviteCustomerService
{
    Task<InviteCustomerResult> InviteCustomerAsyc(InviteCustomerRequest request, CancellationToken cancellationToken = default);

    Task<IList<CustomerRole>> GetInviteRolesAsync();

    Task<InviteCustomerResult> RevokeInviteAsync(string membershipId, CancellationToken cancellationToken = default);

    Task<InviteCustomerResult> ResendInviteAsync(string membershipId, string urlSuffix = null, string message = null, CancellationToken cancellationToken = default);
}
