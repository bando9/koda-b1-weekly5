using Ewallet.Mvc.DTOs;

namespace Ewallet.Mvc.ViewModels;

public class TransactionHistoryViewModel
{
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string WalletNumber { get; set; } = string.Empty;

    public string Filter { get; set; } = "all";

    public IReadOnlyList<TransactionHistoryResponse> Transactions
        { get; set; } = [];
}