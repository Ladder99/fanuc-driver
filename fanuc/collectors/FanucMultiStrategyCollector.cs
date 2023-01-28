using l99.driver.fanuc.strategies;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors;

public class FanucMultiStrategyCollector
{
    protected readonly ILogger Logger;
    protected readonly FanucMultiStrategy Strategy;
    protected dynamic Configuration;

    protected FanucMultiStrategyCollector(FanucMultiStrategy strategy, dynamic configuration)
    {
        Logger = LogManager.GetLogger(GetType().FullName);
        Strategy = strategy;
        Configuration = configuration;

        if (!Configuration.ContainsKey("enabled")) Configuration.Add("enabled", true);
    }

    public virtual async Task InitRootAsync()
    {
        await Task.FromResult(0);
    }

    public virtual async Task InitPathsAsync()
    {
        await Task.FromResult(0);
    }

    public virtual async Task InitAxisAsync()
    {
        await Task.FromResult(0);
    }

    public virtual async Task InitSpindleAsync()
    {
        await Task.FromResult(0);
    }

    public virtual async Task PostInitAsync(Dictionary<string, List<string>> structure)
    {
        await Task.FromResult(0);
    }

    public virtual async Task CollectRootAsync()
    {
        await Task.FromResult(0);
    }

    public virtual async Task CollectForEachPathAsync(short currentPath, string[] axis, string[] spindle,
        dynamic pathMarker)
    {
        await Task.FromResult(0);
    }

    public virtual async Task CollectForEachAxisAsync(short currentPath, short currentAxis, string axisName,
        dynamic axisSplit, dynamic axisMarker)
    {
        await Task.FromResult(0);
    }

    public virtual async Task CollectForEachSpindleAsync(short currentPath, short currentSpindle, string spindleName,
        dynamic spindleSplit, dynamic spindleMarker)
    {
        await Task.FromResult(0);
    }
}