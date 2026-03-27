using System.Collections.Concurrent;
using PolicySearchService.Domain;

namespace PolicySearchService.DataAccess.ElasticSearch;

public class PolicyRepository : IPolicyRepository
{
    private readonly ConcurrentDictionary<string, Policy> policies = new();

    public Task Add(Policy policy)
    {
        policies[policy.PolicyNumber] = policy;
        return Task.CompletedTask;
    }

    public Task<List<Policy>> Find(string queryText)
    {
        if (string.IsNullOrWhiteSpace(queryText))
            return Task.FromResult(policies.Values
                .OrderBy(p => p.PolicyNumber)
                .Take(10)
                .ToList());

        var terms = queryText
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var matches = policies.Values
            .Where(policy => terms.All(term =>
                policy.PolicyNumber.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                policy.PolicyHolder.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(policy => policy.PolicyNumber)
            .Take(10)
            .ToList();

        return Task.FromResult(matches);
    }
}
