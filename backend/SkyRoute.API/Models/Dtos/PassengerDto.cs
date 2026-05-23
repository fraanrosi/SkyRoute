using System.ComponentModel.DataAnnotations;
using SkyRoute.API.Models.Enums;

namespace SkyRoute.API.Models.Dtos;

public class PassengerDto
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(20, MinimumLength = 5)]
    public string DocumentNumber { get; set; } = string.Empty;

    [Required]
    public DocumentType DocumentType { get; set; }
}
