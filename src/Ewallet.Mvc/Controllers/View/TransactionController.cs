using Ewallet.Mvc.Services;
using Ewallet.Mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Ewallet.Mvc.Controllers.View;

public class TransactionController : Controller
{
    private readonly IUserService _userService;
    private readonly IWalletService _walletService;

    public TransactionController(
        IUserService userService,
        IWalletService walletService)
    {
        _userService = userService;
        _walletService = walletService;
    }

    [HttpGet]
    public async Task<IActionResult> History(
        int id,
        string filter = "all",
        CancellationToken cancellationToken = default)
    {
        var user = await _userService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (user is null)
            return NotFound("User tidak ditemukan.");

        var wallet = await _walletService.GetByUserIdAsync(
            id,
            cancellationToken
        );

        if (wallet is null)
            return NotFound("Wallet tidak ditemukan.");

        var transactions = await _userService
            .GetTransactionHistoryResponsesAsync(
                id,
                cancellationToken
            );

        filter = filter.ToLowerInvariant();

        if (filter is not ("all" or "income" or "expense"))
        {
            filter = "all";
        }

        var filteredTransactions = filter switch
        {
            "income" => transactions
                .Where(t => t.IsIncoming)
                .ToList(),

            "expense" => transactions
                .Where(t => !t.IsIncoming)
                .ToList(),

            _ => transactions
        };

        var viewModel = new TransactionHistoryViewModel
        {
            UserId = user.Id,
            FullName = user.FullName,
            WalletNumber = wallet.WalletNumber,
            Filter = filter,
            Transactions = filteredTransactions
        };

        return View(viewModel);
    }
}