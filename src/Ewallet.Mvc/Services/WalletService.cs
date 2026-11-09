using Ewallet.Mvc.DTOs;
using Ewallet.Mvc.Models;
using Ewallet.Mvc.Repositories;
using Microsoft.Data.SqlClient;

namespace Ewallet.Mvc.Services;

public sealed class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;

    public WalletService(IWalletRepository walletRepo)
        {
            _walletRepository = walletRepo;
        }

    // public async Task<TopUpResponse?> CreateAsync(TopUpRequest request, int userId,CancellationToken cancellationToken)
    // {
    //     using var connection = new SqlConnection(_connectionString);
    //     await connection.OpenAsync(cancellationToken);
    //     using var tx = connection.BeginTransaction();

    //     try
    //     {
    //         bool isUpdated = await _walletRepository.UpdateBalanceAsync(userId, request.Amount, connection,tx,cancellationToken);

    //         if (!isUpdated)
    //         {
    //             await tx.RollbackAsync(cancellationToken);
    //             return new TopUpResponse(false, "Wallet user tidak ditemukan.", 0, DateTime.Now);
    //         }
    //         await _transactionRepo.CreateTransactionAsync(null, userId, request.Amount, "TOPUP","TopUpMakan",tx, cancellationToken);

    //     await tx.CommitAsync(cancellationToken);

    //     return new TopUpResponse(true, "Top-up berhasil.", 0, DateTime.Now);
    //     }
    //     catch (OperationCanceledException)
    //     {
    //         await tx.RollbackAsync(CancellationToken.None);
    //         throw;
    //     }
    //     catch (Exception ex)
    //     {
    //         await tx.RollbackAsync(cancellationToken);
    //         return new TopUpResponse(false, $"Gagal: {ex.Message}", 0, DateTime.Now);
    //     }





    //     // var newBalance = 
    //     // var wallet = new Wallet (
    //     //     id:0,
    //     //     walletNumber: "21",
    //     //     userId:2,
    //     //     balance:2
    //     // );
    // }

    public async Task<TopUpResponse> TopUpAsync(int userId, TopUpRequest request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Top up minimal 1, gabisa dibawah itu.");
        }

        var result = await _walletRepository.ProcessTopUpAsync(userId, request.Amount, request.Description, cancellationToken);

        return new TopUpResponse(
            TransactionId: result.TransactionId,
            WalletNumber: result.WalletNumber,
            TopUpAmount: request.Amount,
            NewBalance: result.NewBalance,
            CreatedAt: DateTime.UtcNow
        );
    }
}