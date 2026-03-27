using Microsoft.Extensions.DependencyInjection;
using PolicySearchService.Domain;

namespace PolicySearchService.DataAccess.ElasticSearch;

public static class NestInstaller
{
    public static IServiceCollection AddElasticSearch(this IServiceCollection services, string cnString)
    {
        services.AddSingleton<IPolicyRepository, PolicyRepository>();
        return services;
    }
}
