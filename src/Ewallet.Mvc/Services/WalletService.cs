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

    public async Task<WalletResponse?> GetByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        if(userId <= 0)
        {
            return null;
        }

        var wallet = await _walletRepository.GetByUserId(userId,cancellationToken);

        return wallet is null ? null : WalletResponse.FromModel(wallet);
    }

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