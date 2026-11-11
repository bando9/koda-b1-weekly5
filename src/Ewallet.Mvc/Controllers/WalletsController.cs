using Ewallet.Mvc.DTOs;
using Ewallet.Mvc.Services;
using Ewallet.Mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Ewallet.Mvc.Controllers;

public class WalletController : Controller
{
    private readonly IUserService _userService;
    private readonly IWalletService _walletService;

    public WalletController(
        IUserService userService,
        IWalletService walletService)
    {
        _userService = userService;
        _walletService = walletService;
    }

    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        
        var user = await _userService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (user is null)
        {
            return NotFound();
        }

        
        var wallet = await _walletService.GetByUserIdAsync(
            id,
            cancellationToken
        );

        if (wallet is null)
        {
            return NotFound("Wallet tidak ditemukan.");
        }

        
        var transactions = await _userService
            .GetTransactionHistoryResponsesAsync(
                id,
                cancellationToken
            );

        
        var totalIncome = transactions
            .Where(t => t.IsIncoming)
            .Sum(t => t.Amount);

        var totalExpense = transactions
            .Where(t => !t.IsIncoming)
            .Sum(t => t.Amount);

        
        var viewModel = new WalletDashboardViewModel
        {
            UserId = user.Id,
            FullName = user.FullName,
            WalletNumber = wallet.WalletNumber,
            Balance = wallet.Balance,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> TopUp(
        int id,
        CancellationToken cancellationToken)
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

        var viewModel = new TopUpViewModel
        {
            UserId = user.Id,
            FullName = user.FullName,
            WalletNumber = wallet.WalletNumber
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TopUp(
        int id,
        TopUpViewModel model,
        CancellationToken cancellationToken)
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

        model.UserId = user.Id;
        model.FullName = user.FullName;
        model.WalletNumber = wallet.WalletNumber;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var request = new TopUpRequest(
                model.Amount,
                model.Description
            );

            var result = await _walletService.TopUpAsync(
                id,
                request,
                cancellationToken
            );

            TempData["SuccessMessage"] =
                $"Top up berhasil sebesar Rp{result.TopUpAmount:N0}. " +
                $"Saldo terbaru: Rp{result.NewBalance:N0}.";

            return RedirectToAction(
                nameof(Details),
                new { id }
            );
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(
                nameof(model.Amount),
                ex.Message
            );

            return View(model);
        }
    }


    [HttpGet]
    public async Task<IActionResult> Transfer(
        int id,
        CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        if (user is null) return NotFound("User tidak ditemukan.");

        var wallet = await _walletService.GetByUserIdAsync(id, cancellationToken);
        if (wallet is null) return NotFound("Wallet tidak ditemukan.");

        var viewModel = new TransferViewModel
        {
            UserId = user.Id,
            FullName = user.FullName,
            SenderWalletNumber = wallet.WalletNumber
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transfer(
        int id,
        TransferViewModel model,
        CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        if (user is null) return NotFound("User tidak ditemukan.");

        var wallet = await _walletService.GetByUserIdAsync(id, cancellationToken);
        if (wallet is null) return NotFound("Wallet tidak ditemukan.");

        model.UserId = user.Id;
        model.FullName = user.FullName;
        model.SenderWalletNumber = wallet.WalletNumber;

        if (!ModelState.IsValid)
            return View(model);

        return View("ConfirmTransfer", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmTransfer(
        int id,
        TransferViewModel model,
        CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        if (user is null) return NotFound("User tidak ditemukan.");

        var wallet = await _walletService.GetByUserIdAsync(id, cancellationToken);
        if (wallet is null) return NotFound("Wallet tidak ditemukan.");

        model.UserId = user.Id;
        model.FullName = user.FullName;
        model.SenderWalletNumber = wallet.WalletNumber;

        if (!ModelState.IsValid)
            return View("Transfer", model);

        try
        {
            var request = new TransferRequest(
                model.ReceiverWalletNumber,
                model.Amount,
                model.Description
            );

            await _walletService.TransferAsync(
                id,
                request,
                cancellationToken
            );

            TempData["SuccessMessage"] =
                $"Transfer sebesar Rp{model.Amount:N0} berhasil diproses.";

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(
                nameof(model.ReceiverWalletNumber),
                ex.Message
            );

            return View("Transfer", model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("Transfer", model);
        }
    }



}