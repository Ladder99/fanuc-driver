using l99.driver.@base;
using l99.driver.fanuc.utils;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc;

// ReSharper disable once ClassNeverInstantiated.Global
public class FanucMachine : Machine
{
    private CancellationTokenSource _stoppingSource;
    
    public FanucMachine(Machines machines, object configuration) : base(machines, configuration)
    {
        if (!Configuration.type.ContainsKey("sweep_ms"))
            Configuration.type.Add("sweep_ms", 1000);

        if (!Configuration.type.ContainsKey("net"))
        {
            Configuration.type.Add("net", new Dictionary<object, object>()
            {
                { "ip", "127.0.0.1" },
                { "port", 8193 },
                { "timeout_s", 3 }
            });
        }
        else if (Configuration.type["net"] == null)
        {
            Configuration.type["net"] = new Dictionary<object, object>()
            {
                { "ip", "127.0.0.1" },
                { "port", 8193 },
                { "timeout_s", 3 }
            };
        }

        _stoppingSource = new CancellationTokenSource();

        this["platform"] = new Platform(this, _stoppingSource.Token);

        FocasEndpoint = new FocasEndpoint(
            Configuration.type["net"]["ip"],
            (ushort) Configuration.type["net"]["port"],
            (short) Configuration.type["net"]["timeout_s"]);
    }

    public override async Task Stop()
    {
        Logger.Info($"[{Id}] Platform stopping");
        _stoppingSource.Cancel();

        while (this["platform"]!.IsRunning)
        {
            await Task.Yield();
        }
        
        Logger.Info($"[{Id}] Platform stopped");
    }

    public override dynamic Info =>
        new
        {
            _id = Id,
            FocasEndpoint.IPAddress,
            FocasEndpoint.Port,
            FocasEndpoint.ConnectionTimeout
        };

    public FocasEndpoint FocasEndpoint { get; }

    public override string ToString()
    {
        return new
        {
            Id,
            FocasEndpoint.IPAddress,
            FocasEndpoint.Port,
            FocasEndpoint.ConnectionTimeout
        }.ToString()!;
    }
}