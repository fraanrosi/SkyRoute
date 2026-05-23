using SkyRoute.API.Models.Dtos;
using SkyRoute.API.Models.Entities;
using SkyRoute.API.Models.Enums;

namespace SkyRoute.API.Providers;

internal static class MockFlightGenerator
{
    public static IEnumerable<Flight> Generate(
        string providerName,
        string flightNumberPrefix,
        decimal baseFareFloor,
        decimal baseFareCeiling,
        int flightsToReturn,
        FlightSearchRequest request)
    {
        var seed = HashSeed(providerName, request);
        var rng = new Random(seed);

        var dayStart = request.DepartureDate.Date;
        var cabinMultiplier = request.CabinClass switch
        {
            CabinClass.Business => 2.4m,
            CabinClass.First    => 4.0m,
            _                   => 1.0m
        };

        for (int i = 0; i < flightsToReturn; i++)
        {
            var departureHour = rng.Next(5, 22);
            var departureMinute = rng.Next(0, 60);
            var durationMinutes = rng.Next(75, 600);

            var departure = dayStart.AddHours(departureHour).AddMinutes(departureMinute);
            var duration = TimeSpan.FromMinutes(durationMinutes);
            var arrival = departure.Add(duration);

            var baseFare = Math.Round(
                ((decimal)rng.NextDouble() * (baseFareCeiling - baseFareFloor) + baseFareFloor) * cabinMultiplier,
                2,
                MidpointRounding.AwayFromZero);

            yield return new Flight
            {
                FlightId = $"{providerName}-{request.OriginAirportCode}-{request.DestinationAirportCode}-{dayStart:yyyyMMdd}-{i}",
                AirlineProvider = providerName,
                FlightNumber = $"{flightNumberPrefix}{rng.Next(100, 999)}",
                OriginAirportCode = request.OriginAirportCode.ToUpperInvariant(),
                DestinationAirportCode = request.DestinationAirportCode.ToUpperInvariant(),
                DepartureTime = departure,
                ArrivalTime = arrival,
                Duration = duration,
                CabinClass = request.CabinClass,
                BaseFare = baseFare
            };
        }
    }

    private static int HashSeed(string providerName, FlightSearchRequest request)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + providerName.GetHashCode(StringComparison.Ordinal);
            hash = hash * 31 + request.OriginAirportCode.ToUpperInvariant().GetHashCode(StringComparison.Ordinal);
            hash = hash * 31 + request.DestinationAirportCode.ToUpperInvariant().GetHashCode(StringComparison.Ordinal);
            hash = hash * 31 + request.DepartureDate.Date.GetHashCode();
            hash = hash * 31 + (int)request.CabinClass;
            return hash;
        }
    }
}
