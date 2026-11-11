using System.ComponentModel.DataAnnotations;

namespace Ewallet.Mvc.DTOs;

public record TopUpRequest(
    [Required(ErrorMessage = "Harus diisi angka topupnya.")]
    [Range(typeof(decimal),"1", "1000000000000", ErrorMessage = "Angka minimal 1.")]
    decimal Amount,

    [StringLength(150,ErrorMessage = "Ga boleh lebih dari 150.")]
    string? Description
);