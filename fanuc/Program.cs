using l99.driver.@base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;

namespace l99.driver.fanuc
{
    class Program
    {
        static async Task Main(string[] args)
        {
            System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
            
            var hostBuilder = Host.CreateDefaultBuilder(args);
            
            if (WindowsServiceHelpers.IsWindowsService())
                hostBuilder.UseWindowsService();

            hostBuilder.ConfigureServices((hostContext, services) => 
                {
                    services.AddSingleton(args);
                    services.AddHostedService<FanucService>();
                })
                .Build()
                .Run();
        }
    }

    public class FanucService : BackgroundService
    {
        public FanucService(string[] args)
        {
            _args = args;
        }

        private string[] _args;
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            dynamic config = await Bootstrap.Start(_args);
            Machines machines = await Machines.CreateMachines(config);
            await machines.RunAsync();
            await Bootstrap.Stop();
        }
    }
}      