using SkyRoute.API.Pricing;

namespace SkyRoute.Tests.Pricing;

public class GlobalAirPricingStrategyTests
{
    private readonly GlobalAirPricingStrategy _strategy = new();

    [Theory]
    [InlineData(100.00, 115.00)]
    [InlineData(200.00, 230.00)]
    [InlineData(1.00,   1.15)]
    [InlineData(0.00,   0.00)]
    public void Apply_AddsFifteenPercentFuelSurcharge(decimal baseFare, decimal expected)
    {
        var result = _strategy.Apply(baseFare);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(99.99,  114.99)]
    [InlineData(123.45, 141.97)]
    [InlineData(50.005, 57.51)]
    public void Apply_RoundsToTwoDecimals_AwayFromZero(decimal baseFare, decimal expected)
    {
        var result = _strategy.Apply(baseFare);
        Assert.Equal(expected, result);
    }
}
