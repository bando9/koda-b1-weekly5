using System.ComponentModel.DataAnnotations;

namespace Ewallet.Mvc.ViewModels;

public class TopUpViewModel
{
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string WalletNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nominal top up wajib diisi.")]
    [Range(typeof(decimal), "1", "999999999999",
        ErrorMessage = "Nominal top up minimal Rp1.")]
    public decimal Amount { get; set; }

    [StringLength(255, ErrorMessage = "Deskripsi maksimal 255 karakter.")]
    public string? Description { get; set; }
}