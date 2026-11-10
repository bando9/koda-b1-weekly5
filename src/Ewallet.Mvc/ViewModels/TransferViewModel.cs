
using System.ComponentModel.DataAnnotations;

namespace Ewallet.Mvc.ViewModels;

public class TransferViewModel
{
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string SenderWalletNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nomor wallet tujuan wajib diisi.")]
    public string ReceiverWalletNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nominal transfer wajib diisi.")]
    [Range(typeof(decimal), "1", "999999999999",
        ErrorMessage = "Nominal transfer minimal Rp1.")]
    public decimal Amount { get; set; }

    [StringLength(150, ErrorMessage = "Deskripsi maksimal 150 karakter.")]
    public string? Description { get; set; }
}
