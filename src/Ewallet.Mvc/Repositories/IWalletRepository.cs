using Ewallet.Mvc.Models;
using Microsoft.Data.SqlClient;

namespace Ewallet.Mvc.Repositories;

public interface IWalletRepository
{
    Task<bool> TryTopUpAsync(int userId, decimal amount,SqlTransaction tx, CancellationToken cancellationToken);

    Task<decimal?> GetBalanceByUserIdAsync(int userId, SqlTransaction tx, CancellationToken cancellationToken);

    Task<bool> UpdateBalanceAsync(int userId, decimal amount, SqlConnection connection, SqlTransaction transaction, CancellationToken cancellationToken);

    Task<(int TransactionId, string WalletNumber, decimal NewBalance)> ProcessTopUpAsync(int userId, decimal amount, string? description, CancellationToken cancellationToken);

}



