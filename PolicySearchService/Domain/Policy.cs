using System;

namespace PolicySearchService.Domain;

public class Policy
{
    public string PolicyNumber { get; set; } = string.Empty;
    public DateTimeOffset PolicyStartDate { get; set; }
    public DateTimeOffset PolicyEndDate { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string PolicyHolder { get; set; } = string.Empty;
    public decimal PremiumAmount { get; set; }
}
