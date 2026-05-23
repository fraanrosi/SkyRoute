using SkyRoute.API.Data;
using SkyRoute.API.Models.Dtos;
using SkyRoute.API.Providers;

namespace SkyRoute.API.Services;

public class FlightAggregatorService
{
    private readonly IEnumerable<IFlightProvider> _providers;
    private readonly ILogger<FlightAggregatorService> _logger;

    public FlightAggregatorService(
        IEnumerable<IFlightProvider> providers,
        ILogger<FlightAggregatorService> logger)
    {
        _providers = providers;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FlightResult>> SearchAsync(FlightSearchRequest request, CancellationToken cancellationToken)
    {
        if (!AirportSeed.Airports.Any(a => a.Code.Equals(request.OriginAirportCode, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Unknown origin airport code: {request.OriginAirportCode}");
        if (!AirportSeed.Airports.Any(a => a.Code.Equals(request.DestinationAirportCode, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Unknown destination airport code: {request.DestinationAirportCode}");
        if (string.Equals(request.OriginAirportCode, request.DestinationAirportCode, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Origin and destination must differ.");

        var tasks = _providers.Select(p => SafeSearchAsync(p, request, cancellationToken));
        var results = await Task.WhenAll(tasks);
        return results.SelectMany(r => r).ToList();
    }

    private async Task<IEnumerable<FlightResult>> SafeSearchAsync(
        IFlightProvider provider,
        FlightSearchRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return await provider.SearchAsync(request, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Provider {Provider} failed during search; returning empty result for this provider.", provider.Name);
            return Array.Empty<FlightResult>();
        }
    }
}
