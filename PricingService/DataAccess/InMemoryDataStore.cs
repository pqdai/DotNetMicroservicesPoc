using Microsoft.Extensions.DependencyInjection;
using PricingService.Domain;

namespace PricingService.DataAccess;

public sealed class InMemoryDataStore : IDataStore
{
    private readonly InMemoryTariffRepository tariffs = new();

    public ITariffRepository Tariffs => tariffs;

    public Task CommitChanges()
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }

    private sealed class InMemoryTariffRepository : ITariffRepository
    {
        private readonly Dictionary<string, Tariff> tariffs = new(StringComparer.OrdinalIgnoreCase);

        public Task<Tariff> this[string code] => WithCode(code);

        public void Add(Tariff tariff)
        {
            tariffs[tariff.Code] = tariff;
        }

        public Task<bool> Exists(string code)
        {
            return Task.FromResult(tariffs.ContainsKey(code));
        }

        public Task<Tariff> WithCode(string code)
        {
            if (tariffs.TryGetValue(code, out var tariff))
            {
                return Task.FromResult(tariff);
            }

            throw new KeyNotFoundException($"Tariff with code '{code}' was not found.");
        }
    }
}

public static class InMemoryDataStoreInstaller
{
    public static IServiceCollection AddInMemoryPricingStore(this IServiceCollection services)
    {
        services.AddSingleton<IDataStore, InMemoryDataStore>();
        return services;
    }
}
