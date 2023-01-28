using l99.driver.fanuc.strategies;
using l99.driver.fanuc.veneers;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors;

// ReSharper disable once UnusedType.Global
public class GCodeData : FanucMultiStrategyCollector
{
    public GCodeData(FanucMultiStrategy strategy, object configuration) : base(strategy, configuration)
    {
        if (!Configuration.ContainsKey("block_counter")) Configuration.Add("block_counter", false);

        if (!Configuration.ContainsKey("buffer_length")) Configuration.Add("buffer_length", 512);
    }

    public override async Task InitPathsAsync()
    {
        await Strategy.Apply(typeof(GCodeBlocks), "gcode");
    }

    public override async Task CollectForEachPathAsync(short currentPath, string[] axis, string[] spindle,
        dynamic pathMarker)
    {
        if (Configuration["block_counter"])
            await Strategy.SetNativeKeyed("blkcount",
                await Strategy.Platform.RdBlkCountAsync());
        else
            await Strategy.SetNativeNullKeyed("blkcount");

        await Strategy.Peel("gcode",
            new[]
            {
                Strategy.GetKeyed("blkcount"),
                await Strategy.SetNativeKeyed("actpt",
                    await Strategy.Platform.RdActPtAsync()),
                await Strategy.SetNativeKeyed("execprog",
                    await Strategy.Platform.RdExecProgAsync(Configuration["buffer_length"]))
            },
            new dynamic[]
            {
            });
    }
}