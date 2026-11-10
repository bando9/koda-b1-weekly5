using Ewallet.Mvc.DTOs;
using Ewallet.Mvc.Models;

namespace Ewallet.Mvc.Repositories;

public interface IUserRepository
{
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> TryAddAsync(User user, CancellationToken cancellationToken);
    Task<bool> TryUpdateAsync(User user, CancellationToken cancellationToken);

    Task<bool> TryDeleteAsync(int id, CancellationToken cancellationToken);

    Task<int?> TryAddWithWalletAsync(User user, Wallet wallet, CancellationToken cancellationToken);

    Task<List<TransactionHistoryResponse>> GetTransactionHistoryAsync(int userId, CancellationToken cancellationToken);

    

}