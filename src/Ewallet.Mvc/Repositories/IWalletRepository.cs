using Ewallet.Mvc.DTOs;
using Ewallet.Mvc.Models;
using Microsoft.Data.SqlClient;

namespace Ewallet.Mvc.Repositories;

public interface IWalletRepository
{

    Task<(int TransactionId, string WalletNumber, decimal NewBalance)> ProcessTopUpAsync(int userId, decimal amount, string? description, CancellationToken cancellationToken);
    

    Task<Wallet?> GetByUserId(int userId, CancellationToken cancellationToken);

}

 

