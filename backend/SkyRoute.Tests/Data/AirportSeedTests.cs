using SkyRoute.API.Data;

namespace SkyRoute.Tests.Data;

public class AirportSeedTests
{
    [Fact]
    public void Airports_ContainsSeedSetAcrossMultipleCountries()
    {
        Assert.True(AirportSeed.Airports.Count >= 6, "Challenge requires at least 6 airports");
        var distinctCountries = AirportSeed.Airports.Select(a => a.Country).Distinct().Count();
        Assert.True(distinctCountries >= 2, "Challenge requires at least 2 distinct countries");
    }

    [Theory]
    [InlineData("EZE")]
    [InlineData("eze")] // case-insensitive lookup
    public void FindByCode_ReturnsAirport_IgnoringCase(string code)
    {
        var airport = AirportSeed.FindByCode(code);
        Assert.NotNull(airport);
        Assert.Equal("EZE", airport!.Code);
    }

    [Fact]
    public void FindByCode_ReturnsNull_WhenUnknown()
    {
        Assert.Null(AirportSeed.FindByCode("ZZZ"));
    }

    [Theory]
    [InlineData("EZE", "MIA", true)]  // Argentina → United States
    [InlineData("EZE", "JFK", true)]  // Argentina → United States
    [InlineData("MIA", "JFK", false)] // United States → United States
    [InlineData("AEP", "COR", false)] // Argentina → Argentina
    [InlineData("EZE", "GRU", true)]  // Argentina → Brazil
    public void IsInternational_ComparesCountries(string origin, string destination, bool expected)
    {
        Assert.Equal(expected, AirportSeed.IsInternational(origin, destination));
    }

    [Fact]
    public void IsInternational_ReturnsFalse_WhenEitherCodeIsUnknown()
    {
        Assert.False(AirportSeed.IsInternational("EZE", "ZZZ"));
        Assert.False(AirportSeed.IsInternational("ZZZ", "MIA"));
    }
}
