using System;
using System.Collections.Generic;

namespace PolicyService.Api.Queries.Dto;

public class PolicyDetailsDto
{
    public string Number { get; set; } = string.Empty;
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public string PolicyHolder { get; set; } = string.Empty;
    public decimal TotalPremium { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;

    public List<string> Covers { get; set; } = new();
}
