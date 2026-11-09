namespace Ewallet.Mvc.DTOs;

public sealed record UserWalletResponse(
    UserResponse User,
    string WalletNumber,
    decimal Balance
);