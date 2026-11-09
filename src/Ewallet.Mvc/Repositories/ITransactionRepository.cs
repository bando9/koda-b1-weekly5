using Microsoft.Data.SqlClient;

namespace Ewallet.Mvc.Repositories;

public interface ITransactionRepository
{
    Task<bool> CreateTransactionAsync(int? senderWalletId, int receiverWalletId, decimal amount, string transactionType, string? Description,SqlTransaction tx, CancellationToken cancellationToken);
    
}