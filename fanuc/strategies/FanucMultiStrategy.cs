using l99.driver.@base;
using l99.driver.fanuc.collectors;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.strategies;

// ReSharper disable once ClassNeverInstantiated.Global
public class FanucMultiStrategy : FanucExtendedStrategy
{
    private readonly List<FanucMultiStrategyCollector> _collectors = new();
    private readonly string _exclusionWildcard = "%";
    private Dictionary<object, dynamic> _exclusions = new();

    public FanucMultiStrategy(Machine machine) : base(machine)
    {
    }

    public override async Task<dynamic?> CreateAsync()
    {
        if (!Machine.Configuration.strategy.ContainsKey("stay_connected"))
        {
            Machine.Configuration.strategy.Add("stay_connected", false);
        }
        
        if (!Machine.Configuration.strategy.ContainsKey("exclusions"))
            Machine.Configuration.strategy.Add("exclusions", new Dictionary<object, object>());
        else if (Machine.Configuration.strategy["exclusions"] == null)
            Machine.Configuration.strategy["exclusions"] = new Dictionary<object, object>();

        _exclusions = Machine.Configuration.strategy["exclusions"];

        if (!Machine.Configuration.strategy.ContainsKey("collectors"))
            Machine.Configuration.strategy.Add("collectors", new List<object>());

        foreach (var collectorType in Machine.Configuration.strategy["collectors"])
        {
            Logger.Info($"[{Machine.Id}] Creating collector: {collectorType}");
            var type = Type.GetType(collectorType);
            try
            {
                var collector = (FanucMultiStrategyCollector) Activator
                    .CreateInstance(type, new object[]
                    {
                        this,
                        Machine.Configuration.collectors.ContainsKey(collectorType)
                            ? Machine.Configuration.collectors[collectorType]
                            : new Dictionary<object, object>()
                    });
                _collectors.Add(collector);
            }
            catch
            {
                Logger.Error($"[{Machine.Id}] Unable to create collector: {collectorType}");
            }
        }

        return null;
    }

    protected override async Task InitRootAsync()
    {
        foreach (var collector in _collectors) await collector.InitRootAsync();
    }

    protected override async Task InitPathsAsync()
    {
        foreach (var collector in _collectors) await collector.InitPathsAsync();
    }

    protected override async Task InitAxisAsync()
    {
        foreach (var collector in _collectors) await collector.InitAxisAsync();
    }

    protected override async Task InitSpindleAsync()
    {
        foreach (var collector in _collectors) await collector.InitSpindleAsync();
    }

    protected override async Task<dynamic?> CollectAsync()
    {
        // user code before starting sweep

        // must call base to continue sweep
        return await base.CollectAsync();
    }

    protected override async Task CollectBeginAsync()
    {
        // user code before connect

        // must call base to connect to machine and return result
        await base.CollectBeginAsync();
        
        // user code after connect, connection result is stored in Get("connect").success
    }

    protected override async Task CollectRootAsync()
    {
        foreach (var collector in _collectors) await collector.CollectRootAsync();
    }

    protected override async Task CollectForEachPathAsync(short currentPath, string[] axis, string[] spindle,
        dynamic pathMarker)
    {
        if ((_exclusions.ContainsKey(currentPath.ToString()) && _exclusions[currentPath.ToString()] == null) ||
            (_exclusions.ContainsKey(currentPath.ToString()) && _exclusions[currentPath.ToString()] != null &&
             _exclusions[currentPath.ToString()]!.Contains(_exclusionWildcard)))
        {
            // entire path is excluded
        }
        else
        {
            foreach (var collector in _collectors)
                await collector.CollectForEachPathAsync(currentPath, axis, spindle, pathMarker);
        }
    }

    protected override async Task CollectForEachAxisAsync(short currentPath, short currentAxis, string axisName,
        dynamic axisSplit, dynamic axisMarker)
    {
        if (_exclusions.ContainsKey(currentPath.ToString()) &&
            _exclusions[currentPath.ToString()] != null &&
            (_exclusions[currentPath.ToString()]!.Contains(_exclusionWildcard) ||
             _exclusions[currentPath.ToString()]!.Contains(axisName)))
        {
            // entire path or specific axis is excluded
        }
        else
        {
            foreach (var collector in _collectors)
                await collector.CollectForEachAxisAsync(currentPath, currentAxis, axisName, axisSplit, axisMarker);
        }
    }

    protected override async Task CollectForEachSpindleAsync(short currentPath, short currentSpindle,
        string spindleName, dynamic spindleSplit, dynamic spindleMarker)
    {
        if (_exclusions.ContainsKey(currentPath.ToString()) &&
            _exclusions[currentPath.ToString()] != null &&
            (_exclusions[currentPath.ToString()]!.Contains(_exclusionWildcard) ||
             _exclusions[currentPath.ToString()]!.Contains(spindleName)))
        {
            // entire path or specific spindle is excluded
        }
        else
        {
            foreach (var collector in _collectors)
                await collector.CollectForEachSpindleAsync(currentPath, currentSpindle, spindleName, spindleSplit,
                    spindleMarker);
        }
    }

    protected override async Task CollectEndAsync()
    {
        // user code before disconnect

        // must call base to disconnect machine
        await base.CollectEndAsync();

        // user code after disconnect, result is stored in Get("disconnect").success
    }
}