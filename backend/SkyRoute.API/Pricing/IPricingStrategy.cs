namespace SkyRoute.API.Pricing;

public interface IPricingStrategy
{
    decimal Apply(decimal baseFare);
}
