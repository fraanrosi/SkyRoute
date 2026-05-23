using SkyRoute.API.Data;
using SkyRoute.API.Models.Dtos;
using SkyRoute.API.Pricing;

namespace SkyRoute.API.Providers;

public class GlobalAirProvider : IFlightProvider
{
    public string Name => "GlobalAir";

    private readonly IPricingStrategy _pricingStrategy;
    private readonly InMemoryFlightStore _flightStore;
    private readonly ILogger<GlobalAirProvider> _logger;

    public GlobalAirProvider(
        GlobalAirPricingStrategy pricingStrategy,
        InMemoryFlightStore flightStore,
        ILogger<GlobalAirProvider> logger)
    {
        _pricingStrategy = pricingStrategy;
        _flightStore = flightStore;
        _logger = logger;
    }

    public Task<IEnumerable<FlightResult>> SearchAsync(FlightSearchRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("{Provider} searching {Origin}->{Destination} on {Date}",
            Name, request.OriginAirportCode, request.DestinationAirportCode, request.DepartureDate.Date);

        var flights = MockFlightGenerator
            .Generate(
                providerName: Name,
                flightNumberPrefix: "GA",
                baseFareFloor: 180m,
                baseFareCeiling: 480m,
                flightsToReturn: 3,
                request: request)
            .ToList();

        var results = new List<FlightResult>(flights.Count);
        foreach (var flight in flights)
        {
            flight.PricePerPassenger = _pricingStrategy.Apply(flight.BaseFare);
            _flightStore.Upsert(flight);

            results.Add(new FlightResult
            {
                FlightId = flight.FlightId,
                AirlineProvider = flight.AirlineProvider,
                FlightNumber = flight.FlightNumber,
                OriginAirportCode = flight.OriginAirportCode,
                DestinationAirportCode = flight.DestinationAirportCode,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                Duration = flight.Duration,
                CabinClass = flight.CabinClass,
                PricePerPassenger = flight.PricePerPassenger,
                TotalPrice = flight.PricePerPassenger * request.NumberOfPassengers,
                NumberOfPassengers = request.NumberOfPassengers
            });
        }

        return Task.FromResult<IEnumerable<FlightResult>>(results);
    }
}
