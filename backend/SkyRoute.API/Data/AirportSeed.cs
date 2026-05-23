using SkyRoute.API.Models.Dtos;

namespace SkyRoute.API.Data;

public static class AirportSeed
{
    public static IReadOnlyList<Airport> Airports { get; } = new[]
    {
        new Airport { Code = "EZE", Name = "Ministro Pistarini",       City = "Buenos Aires", Country = "Argentina"     },
        new Airport { Code = "AEP", Name = "Aeroparque Jorge Newbery", City = "Buenos Aires", Country = "Argentina"     },
        new Airport { Code = "COR", Name = "Ingeniero Taravella",      City = "Córdoba",      Country = "Argentina"     },
        new Airport { Code = "MIA", Name = "Miami International",      City = "Miami",        Country = "United States" },
        new Airport { Code = "JFK", Name = "John F. Kennedy",          City = "New York",     Country = "United States" },
        new Airport { Code = "GRU", Name = "São Paulo–Guarulhos",      City = "São Paulo",    Country = "Brazil"        },
        new Airport { Code = "SCL", Name = "Arturo Merino Benítez",    City = "Santiago",     Country = "Chile"         }
    };

    public static Airport? FindByCode(string code)
        => Airports.FirstOrDefault(a => string.Equals(a.Code, code, StringComparison.OrdinalIgnoreCase));

    public static bool IsInternational(string originCode, string destinationCode)
    {
        var origin = FindByCode(originCode);
        var destination = FindByCode(destinationCode);
        if (origin is null || destination is null) return false;
        return !string.Equals(origin.Country, destination.Country, StringComparison.OrdinalIgnoreCase);
    }
}
