using System;
using MediatR;

namespace PolicyService.Api.Commands;

public class TerminatePolicyCommand : IRequest<TerminatePolicyResult>
{
    public string PolicyNumber { get; set; } = string.Empty;
    public DateTime TerminationDate { get; set; }
}
