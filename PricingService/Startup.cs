using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using PricingService.Configuration;
using PricingService.DataAccess;
using PricingService.Infrastructure;
using PricingService.Init;
using Steeltoe.Discovery.Client;


namespace PricingService;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDiscoveryClient(Configuration);
        services.AddControllers()
            .AddNewtonsoftJson(opt => { opt.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto; });

        services.AddInMemoryPricingStore();
        services.AddPricingDemoInitializer();
        services.AddMediatR(options => options.RegisterServicesFromAssemblyContaining<Program>());
        services.AddLoggingBehavior();
        services.AddSwaggerGen();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {

        app.UseRouting();
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var feature = context.Features.Get<IExceptionHandlerFeature>();
                if (feature?.Error is not null)
                {
                    await ExceptionMapper.WriteExceptionResponseAsync(context, feature.Error);
                }
            });
        });

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseInitializer();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
