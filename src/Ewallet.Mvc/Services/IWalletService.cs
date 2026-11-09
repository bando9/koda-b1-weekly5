using Ewallet.Mvc.DTOs;

namespace Ewallet.Mvc.Services;

public interface IWalletService
{
    Task<WalletResponse?> GetByUserIdAsync(int userId, CancellationToken cancellationToken);
    Task<TopUpResponse> TopUpAsync(int userId, TopUpRequest request,CancellationToken cancellationToken);
}