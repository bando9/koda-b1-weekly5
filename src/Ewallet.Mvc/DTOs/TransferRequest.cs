using System.ComponentModel.DataAnnotations;

namespace Ewallet.Mvc.DTOs;

public record TransferRequest(
    [Required(ErrorMessage = "Nomor wallet tujuan wajib diisi.")]
    string ReceiverWalletNumber,

    [Range(typeof(decimal), "1", "999999999999",
        ErrorMessage = "Nominal transfer minimal Rp1.")]
    decimal Amount,

    [StringLength(150, ErrorMessage = "Deskripsi maksimal 150 karakter.")]
    string? Description
);