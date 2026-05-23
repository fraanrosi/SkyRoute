using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SkyRoute.API.Models.Dtos;
using SkyRoute.API.Models.Enums;
using SkyRoute.API.Providers;
using SkyRoute.API.Services;

namespace SkyRoute.Tests.Services;

public class FlightAggregatorServiceTests
{
    private static FlightSearchRequest ValidRequest() => new()
    {
        OriginAirportCode = "EZE",
        DestinationAirportCode = "MIA",
        DepartureDate = DateTime.UtcNow.Date.AddDays(7),
        NumberOfPassengers = 2,
        CabinClass = CabinClass.Economy
    };

    private static FlightResult MakeResult(string provider, decimal price) => new()
    {
        FlightId = $"{provider}-EZE-MIA-1",
        AirlineProvider = provider,
        FlightNumber = $"{provider[..2].ToUpperInvariant()}100",
        OriginAirportCode = "EZE",
        DestinationAirportCode = "MIA",
        DepartureTime = DateTime.UtcNow.Date.AddDays(7).AddHours(8),
        ArrivalTime = DateTime.UtcNow.Date.AddDays(7).AddHours(18),
        Duration = TimeSpan.FromHours(10),
        CabinClass = CabinClass.Economy,
        PricePerPassenger = price,
        TotalPrice = price * 2,
        NumberOfPassengers = 2
    };

    [Fact]
    public async Task SearchAsync_ReturnsResultsFromAllProviders()
    {
        var providerA = new Mock<IFlightProvider>();
        providerA.SetupGet(p => p.Name).Returns("ProviderA");
        providerA.Setup(p => p.SearchAsync(It.IsAny<FlightSearchRequest>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new[] { MakeResult("ProviderA", 100m) });

        var providerB = new Mock<IFlightProvider>();
        providerB.SetupGet(p => p.Name).Returns("ProviderB");
        providerB.Setup(p => p.SearchAsync(It.IsAny<FlightSearchRequest>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new[] { MakeResult("ProviderB", 80m) });

        var aggregator = new FlightAggregatorService(
            new[] { providerA.Object, providerB.Object },
            NullLogger<FlightAggregatorService>.Instance);

        var results = await aggregator.SearchAsync(ValidRequest(), CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.AirlineProvider == "ProviderA");
        Assert.Contains(results, r => r.AirlineProvider == "ProviderB");
    }

    [Fact]
    public async Task SearchAsync_IsolatesFailures_OneProviderThrows_OthersStillReturn()
    {
        var healthy = new Mock<IFlightProvider>();
        healthy.SetupGet(p => p.Name).Returns("Healthy");
        healthy.Setup(p => p.SearchAsync(It.IsAny<FlightSearchRequest>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new[] { MakeResult("Healthy", 100m) });

        var failing = new Mock<IFlightProvider>();
        failing.SetupGet(p => p.Name).Returns("Failing");
        failing.Setup(p => p.SearchAsync(It.IsAny<FlightSearchRequest>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new InvalidOperationException("upstream timeout"));

        var aggregator = new FlightAggregatorService(
            new[] { healthy.Object, failing.Object },
            NullLogger<FlightAggregatorService>.Instance);

        var results = await aggregator.SearchAsync(ValidRequest(), CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Healthy", results[0].AirlineProvider);
    }

    [Fact]
    public async Task SearchAsync_RejectsUnknownOriginCode()
    {
        var aggregator = new FlightAggregatorService(
            Array.Empty<IFlightProvider>(),
            NullLogger<FlightAggregatorService>.Instance);

        var request = ValidRequest();
        request.OriginAirportCode = "ZZZ";

        await Assert.ThrowsAsync<ArgumentException>(
            () => aggregator.SearchAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task SearchAsync_RejectsSameOriginAndDestination()
    {
        var aggregator = new FlightAggregatorService(
            Array.Empty<IFlightProvider>(),
            NullLogger<FlightAggregatorService>.Instance);

        var request = ValidRequest();
        request.DestinationAirportCode = request.OriginAirportCode;

        await Assert.ThrowsAsync<ArgumentException>(
            () => aggregator.SearchAsync(request, CancellationToken.None));
    }
}
