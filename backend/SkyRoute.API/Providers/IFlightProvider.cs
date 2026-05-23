using SkyRoute.API.Models.Dtos;

namespace SkyRoute.API.Providers;

public interface IFlightProvider
{
    string Name { get; }

    Task<IEnumerable<FlightResult>> SearchAsync(FlightSearchRequest request, CancellationToken cancellationToken);
}
