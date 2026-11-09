using Ewallet.Mvc.Models;

namespace Ewallet.Mvc.DTOs;

public sealed record TopUpResponse(
    int TransactionId,
    string WalletNumber,
    decimal TopUpAmount,
    decimal NewBalance,
    DateTime CreatedAt
);
