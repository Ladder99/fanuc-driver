#pragma warning disable CS1998

using l99.driver.@base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class Program
    {
        static async Task Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            
            var hostBuilder = Host.CreateDefaultBuilder(args);
            
            if (WindowsServiceHelpers.IsWindowsService())
                hostBuilder.UseWindowsService();

            // ReSharper disable once UnusedParameter.Local
            await hostBuilder.ConfigureServices((hostContext, services) => 
                {
                    services.AddSingleton(args);
                    services.AddHostedService<FanucService>();
                })
                .Build()
                .RunAsync();
        }
    }

    public class FanucService : BackgroundService
    {
        public FanucService(string[] args)
        {
            _args = args;
        }

        private readonly string[] _args;
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            dynamic config = await Bootstrap.Start(_args);
            Machines machines = await Machines.CreateMachines(config);
            await machines.RunAsync(stoppingToken);
            await Bootstrap.Stop();
        }
    }
}
#pragma warning restore CS1998