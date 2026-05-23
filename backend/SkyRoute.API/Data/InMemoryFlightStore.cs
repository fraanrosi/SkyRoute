using System.Collections.Concurrent;
using SkyRoute.API.Models.Entities;

namespace SkyRoute.API.Data;

public class InMemoryFlightStore
{
    private readonly ConcurrentDictionary<string, Flight> _flights = new();

    public void Upsert(Flight flight) => _flights[flight.FlightId] = flight;

    public Flight? Get(string flightId)
        => _flights.TryGetValue(flightId, out var f) ? f : null;
}
