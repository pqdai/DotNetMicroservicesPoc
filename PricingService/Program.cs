using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace PricingService;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            CreateWebHostBuilder(args)
                .Build()
                .Run();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Host terminated unexpectedly: {ex}");
        }
    }

    public static IHostBuilder CreateWebHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
