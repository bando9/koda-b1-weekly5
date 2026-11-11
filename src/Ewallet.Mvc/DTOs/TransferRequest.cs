using System.ComponentModel.DataAnnotations;

namespace Ewallet.Mvc.DTOs;

public record TransferRequest(
    [Required(ErrorMessage = "Nomor wallet tujuan wajib diisi.")]
    [RegularExpression(@"^\d+$",
    ErrorMessage = "Nomor wallet hanya boleh berisi angka.")]
    string ReceiverWalletNumber,

    [Range(typeof(decimal), "1", "999999999999",
        ErrorMessage = "Nominal transfer harus antara Rp1 sampai Rp999.999.999.999.")]
    decimal Amount,

    [StringLength(150, ErrorMessage = "Deskripsi maksimal 150 karakter.")]
    string? Description
);