using SkyRoute.API.Pricing;

namespace SkyRoute.Tests.Pricing;

public class BudgetWingsPricingStrategyTests
{
    private readonly BudgetWingsPricingStrategy _strategy = new();

    [Theory]
    [InlineData(100.00, 90.00)]
    [InlineData(200.00, 180.00)]
    [InlineData(50.00,  45.00)]
    public void Apply_AppliesTenPercentDiscount_WhenAboveFloor(decimal baseFare, decimal expected)
    {
        var result = _strategy.Apply(baseFare);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0.00,  29.99)]
    [InlineData(10.00, 29.99)]
    [InlineData(33.32, 29.99)] // 33.32 * 0.9 = 29.988 → rounds to 29.99 → floor wins (tie)
    public void Apply_EnforcesMinimumPrice_OfTwentyNineNinetyNine(decimal baseFare, decimal expected)
    {
        var result = _strategy.Apply(baseFare);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Apply_JustAboveFloor_DoesNotClampToFloor()
    {
        // 33.33 * 0.9 = 29.997 → rounds (away from zero) to 30.00 → above floor, returns 30.00
        var result = _strategy.Apply(33.33m);
        Assert.Equal(30.00m, result);
    }

    [Theory]
    [InlineData(99.99,  89.99)]
    [InlineData(123.45, 111.11)]
    [InlineData(55.555, 50.00)]
    public void Apply_RoundsToTwoDecimals_AwayFromZero(decimal baseFare, decimal expected)
    {
        var result = _strategy.Apply(baseFare);
        Assert.Equal(expected, result);
    }
}
