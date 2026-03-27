namespace PolicyService.Api.Commands;

public class TerminatePolicyResult
{
    public string PolicyNumber { get; set; } = string.Empty;
    public decimal MoneyToReturn { get; set; }
}
