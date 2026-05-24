using SkyRoute.API.Models.Enums;

namespace SkyRoute.API.Models.Dtos;

public class BookingListItem
{
    public string BookingReference { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public BookingStatus Status { get; set; }
    public decimal TotalPriceCharged { get; set; }

    public string AirlineProvider { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;

    public string OriginAirportCode { get; set; } = string.Empty;
    public string DestinationAirportCode { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }

    public int NumberOfPassengers { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
}
