namespace Ewallet.Mvc.ViewModels;

public class WalletDashboardViewModel
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string WalletNumber { get; set; } = string.Empty;

    public decimal Balance { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
}