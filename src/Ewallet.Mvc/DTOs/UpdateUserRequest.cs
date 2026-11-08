using System.ComponentModel.DataAnnotations;

namespace Ewallet.Mvc.DTOs;

public record UpdateUserRequest(
    [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
    [StringLength(100, ErrorMessage = "Nama lengkap maksimal 100 karakter.")]
    string FullName,

    [Required(ErrorMessage = "Email wajib diisi.")]
    [EmailAddress(ErrorMessage = "Format email tidak valid.")]
    [StringLength(100, ErrorMessage = "Email maksimal 100 karakter.")]
    string Email,

    [Required(ErrorMessage = "Nomor telepon wajib diisi.")]
    [Phone(ErrorMessage = "Format nomor telepon tidak valid.")]
    [StringLength(20, ErrorMessage = "Nomor telepon maksimal 20 karakter.")]
    string Phone,

    [Url(ErrorMessage = "Format URL foto tidak valid.")]
    [StringLength(255, ErrorMessage = "Link photo maksimal 255 karakter.")]
    string? Photo
);