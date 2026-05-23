using SkyRoute.API.Models.Enums;

namespace SkyRoute.API.Models.Entities;

public class Flight
{
    public string FlightId { get; set; } = string.Empty;
    public string AirlineProvider { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string OriginAirportCode { get; set; } = string.Empty;
    public string DestinationAirportCode { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public TimeSpan Duration { get; set; }
    public CabinClass CabinClass { get; set; }

    public decimal BaseFare { get; set; }

    public decimal PricePerPassenger { get; set; }
}
