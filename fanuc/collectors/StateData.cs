using l99.driver.fanuc.strategies;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors;

// ReSharper disable once UnusedType.Global
public class StateData : FanucMultiStrategyCollector
{
    public StateData(FanucMultiStrategy strategy, object configuration) : base(strategy, configuration)
    {
    }

    public override async Task InitPathsAsync()
    {
        await Strategy.Apply(typeof(veneers.StateData), "state", true);
    }

    public override async Task CollectRootAsync()
    {
        await Strategy.SetNative("poweron_time_min",
            await Strategy.Platform.RdParamDoubleWordNoAxisAsync(6750));

        await Strategy.SetNative("operating_time_min",
            await Strategy.Platform.RdParamDoubleWordNoAxisAsync(6752));

        await Strategy.SetNative("cutting_time_min",
            await Strategy.Platform.RdParamDoubleWordNoAxisAsync(6754));
    }

    public override async Task CollectForEachPathAsync(short currentPath, string[] axis, string[] spindle,
        dynamic pathMarker)
    {
        await Strategy.Peel("state",
            new dynamic[]
            {
                await Strategy.SetNativeKeyed("stat_info",
                    await Strategy.Platform.StatInfoAsync()),
                Strategy.Get("poweron_time_min")!,
                Strategy.Get("operating_time_min")!,
                Strategy.Get("cutting_time_min")!,
                await Strategy.SetNativeKeyed("feed_override",
                    await Strategy.Platform.RdPmcRngGByteAsync(12)),
                await Strategy.SetNativeKeyed("rapid_override",
                    await Strategy.Platform.RdPmcRngGByteAsync(14)),
                await Strategy.SetNativeKeyed("spindle_override",
                    await Strategy.Platform.RdPmcRngGByteAsync(30)),
                await Strategy.SetNativeKeyed("modal_m1",
                    await Strategy.Platform.ModalAsync(106, 0, 3)),
                await Strategy.SetNativeKeyed("modal_m2",
                    await Strategy.Platform.ModalAsync(125, 0, 3)),
                await Strategy.SetNativeKeyed("modal_m3",
                    await Strategy.Platform.ModalAsync(126, 0, 3)),
                await Strategy.SetNativeKeyed("modal_t",
                    await Strategy.Platform.ModalAsync(108, 0, 3))
            },
            new dynamic[]
            {
            });
    }
}