using static System.String;

namespace PricingService.Domain;

public class BasePremiumCalculationRule
{
    public BasePremiumCalculationRule(string coverCode, string? applyIfFormula, string basePriceFormula)
    {
        CoverCode = coverCode;
        ApplyIfFormula = applyIfFormula;
        BasePriceFormula = basePriceFormula;
    }

    public string CoverCode { get; }
    public string? ApplyIfFormula { get; }
    public string BasePriceFormula { get; }

    public bool Applies(Cover cover, Calculation calculation)
    {
        if (cover.Code != CoverCode)
            return false;

        if (IsNullOrEmpty(ApplyIfFormula))
            return true;

        return CalculationExpressionEvaluator.EvaluateBoolean(ApplyIfFormula, calculation);
    }

    public decimal CalculateBasePrice(Calculation calculation)
    {
        return CalculationExpressionEvaluator.EvaluateDecimal(BasePriceFormula, calculation);
    }
}
