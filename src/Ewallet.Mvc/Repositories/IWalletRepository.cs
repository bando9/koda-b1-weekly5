using Ewallet.Mvc.DTOs;
using Ewallet.Mvc.Models;
using Microsoft.Data.SqlClient;

namespace Ewallet.Mvc.Repositories;

public interface IWalletRepository
{

    Task<(int TransactionId, string WalletNumber, decimal NewBalance)> ProcessTopUpAsync(int userId, decimal amount, string? description, CancellationToken cancellationToken);
    

    Task<Wallet?> GetByUserId(int userId, CancellationToken cancellationToken);

    Task<Wallet?> GetByWalletNumberAsync(
    string walletNumber,
    CancellationToken cancellationToken
    );

    Task<(
        int TransactionId,
        string SenderWalletNumber,
        string ReceiverWalletNumber,
        decimal SenderNewBalance,
        DateTime CreatedAt
    )> ProcessTransferAsync(
        int senderUserId,
        string receiverWalletNumber,
        decimal amount,
        string? description,
        CancellationToken cancellationToken
    );

}

 

