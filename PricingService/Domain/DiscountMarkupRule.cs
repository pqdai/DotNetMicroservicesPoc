using static System.Decimal;
using static System.String;

namespace PricingService.Domain;

public abstract class DiscountMarkupRule
{
    public string ApplyIfFormula { get; protected set; } = string.Empty;
    public decimal ParamValue { get; protected set; }

    public bool Applies(Calculation calculation)
    {
        if (IsNullOrEmpty(ApplyIfFormula))
            return true;

        return CalculationExpressionEvaluator.EvaluateBoolean(ApplyIfFormula, calculation);
    }

    public abstract Calculation Apply(Calculation calculation);
}

public class PercentMarkupRule : DiscountMarkupRule
{
    public PercentMarkupRule(string applyIfFormula, decimal paramValue)
    {
        ApplyIfFormula = applyIfFormula;
        ParamValue = paramValue;
    }

    public override Calculation Apply(Calculation calculation)
    {
        foreach (var cover in calculation.Covers.Values)
        {
            var priceAfterMarkup = Round(cover.Price * ParamValue, 2);
            cover.SetPrice(priceAfterMarkup);
        }

        return calculation;
    }
}
