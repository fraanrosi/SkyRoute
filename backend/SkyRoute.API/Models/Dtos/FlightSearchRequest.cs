using System.ComponentModel.DataAnnotations;
using SkyRoute.API.Models.Enums;

namespace SkyRoute.API.Models.Dtos;

public class FlightSearchRequest
{
    [Required, StringLength(3, MinimumLength = 3)]
    public string OriginAirportCode { get; set; } = string.Empty;

    [Required, StringLength(3, MinimumLength = 3)]
    public string DestinationAirportCode { get; set; } = string.Empty;

    [Required]
    public DateTime DepartureDate { get; set; }

    [Range(1, 9)]
    public int NumberOfPassengers { get; set; }

    public CabinClass CabinClass { get; set; } = CabinClass.Economy;
}
