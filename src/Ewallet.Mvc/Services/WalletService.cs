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
        if(userId <=0 ) throw new ArgumentException("User Id tidak valid.");

        if (request.Amount < 1)
        {
            throw new ArgumentException("Top up minimal 1, gabisa dibawah itu.");
        }

        var result = await _walletRepository.ProcessTopUpAsync(userId, request.Amount, request.Description, cancellationToken);

        return new TopUpResponse(
            TransactionId: result.TransactionId,
            WalletNumber: result.WalletNumber,
            TopUpAmount: request.Amount,
            NewBalance: result.NewBalance,
            CreatedAt: result.CreatedAt
        );
    }


    public async Task<TransferResponse> TransferAsync(
    int senderUserId,
    TransferRequest request,
    CancellationToken cancellationToken)
    {
        if (senderUserId <= 0)
            throw new ArgumentException("User pengirim tidak valid.");

        if (request.Amount < 1)
            throw new ArgumentException("Nominal transfer minimal Rp1.");

        if (string.IsNullOrWhiteSpace(request.ReceiverWalletNumber))
            throw new ArgumentException("Nomor wallet tujuan wajib diisi.");

        var senderWallet = await _walletRepository.GetByUserId(
            senderUserId,
            cancellationToken
        );

        if (senderWallet is null)
            throw new InvalidOperationException("Wallet pengirim tidak ditemukan.");

        var receiverWallet = await _walletRepository.GetByWalletNumberAsync(
            request.ReceiverWalletNumber.Trim(),
            cancellationToken
        );

        if (receiverWallet is null)
            throw new ArgumentException("Wallet penerima tidak ditemukan.");

        if (senderWallet.Id == receiverWallet.Id)
            throw new ArgumentException("Tidak dapat transfer ke wallet sendiri.");

        var result = await _walletRepository.ProcessTransferAsync(
            senderUserId,
            request.ReceiverWalletNumber.Trim(),
            request.Amount,
            request.Description,
            cancellationToken
        );

        return new TransferResponse(
            TransactionId: result.TransactionId,
            SenderWalletNumber: result.SenderWalletNumber,
            ReceiverWalletNumber: result.ReceiverWalletNumber,
            Amount: request.Amount,
            SenderNewBalance: result.SenderNewBalance,
            CreatedAt: result.CreatedAt
        );
    }


}