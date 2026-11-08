
using Ewallet.Mvc.Models;

namespace Ewallet.Mvc.DTOs;

public sealed record WalletResponse(
    int Id,
    string WalletNumber,
    string PINHash,
    decimal Balance,
    int UserId
){
    public WalletResponse FromModel(Wallet wallet) => new(
        wallet.Id,
        wallet.WalletNumber,
        wallet.PINHash,
        wallet.Balance,
        wallet.UserId
    );
}

