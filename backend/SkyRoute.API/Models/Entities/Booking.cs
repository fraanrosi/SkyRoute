using SkyRoute.API.Models.Dtos;
using SkyRoute.API.Models.Enums;

namespace SkyRoute.API.Models.Entities;

public class Booking
{
    public string BookingReference { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public string FlightId { get; set; } = string.Empty;
    public List<PassengerDto> Passengers { get; set; } = new();
    public string ContactEmail { get; set; } = string.Empty;
    public decimal TotalPriceCharged { get; set; }
    public BookingStatus Status { get; set; }
}
