#pragma warning disable CS1998

using System.IO;
using l99.driver.@base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc;

// ReSharper disable once ClassNeverInstantiated.Global
internal class Program
{
    private static async Task Main(string[] args)
    {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

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
    private readonly string[] _args;

    public FanucService(string[] args)
    {
        _args = args;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = await Bootstrap.Start(_args);
        Machines machines = await Machines.CreateMachines(config);
        await machines.RunAsync(stoppingToken);
        await Bootstrap.Stop();
    }
}
#pragma warning restore CS1998