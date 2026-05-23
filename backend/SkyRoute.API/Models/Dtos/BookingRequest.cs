using System.ComponentModel.DataAnnotations;

namespace SkyRoute.API.Models.Dtos;

public class BookingRequest
{
    [Required]
    public string FlightId { get; set; } = string.Empty;

    [Required, MinLength(1)]
    public List<PassengerDto> Passengers { get; set; } = new();

    [Required, EmailAddress]
    public string ContactEmail { get; set; } = string.Empty;
}
