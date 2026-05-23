using SkyRoute.API.Models.Enums;

namespace SkyRoute.API.Models.Dtos;

public class BookingResponse
{
    public string BookingReference { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public string FlightId { get; set; } = string.Empty;
    public decimal TotalPriceCharged { get; set; }
    public BookingStatus Status { get; set; }
}
