namespace SkyRoute.API.Pricing;

public class GlobalAirPricingStrategy : IPricingStrategy
{
    private const decimal FuelSurchargeMultiplier = 1.15m;

    public decimal Apply(decimal baseFare)
        => Math.Round(baseFare * FuelSurchargeMultiplier, 2, MidpointRounding.AwayFromZero);
}
