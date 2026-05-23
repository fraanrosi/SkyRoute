namespace SkyRoute.API.Pricing;

public class BudgetWingsPricingStrategy : IPricingStrategy
{
    private const decimal PromoMultiplier = 0.90m;
    private const decimal MinimumPrice = 29.99m;

    public decimal Apply(decimal baseFare)
        => Math.Max(Math.Round(baseFare * PromoMultiplier, 2, MidpointRounding.AwayFromZero), MinimumPrice);
}
