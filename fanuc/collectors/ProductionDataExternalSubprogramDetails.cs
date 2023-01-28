using l99.driver.fanuc.strategies;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors;

/// <summary>
///     Some NCs have a PC control that allows mapping of programs from C drive.
///     These programs are not stored in NC memory and can only be retrieved from the C drive.
///     This collector extends the ProductionData collector by interrogating files on the C drive when a specific program
///     number becomes active.
///     Note that subprograms loaded from C drive by NC must have a unique program number (ie. O4321) that can be mapped to
///     a file on the C drive.
/// </summary>
// ReSharper disable once UnusedType.Global
public class ProductionDataExternalSubprogramDetails : FanucMultiStrategyCollector
{
    public ProductionDataExternalSubprogramDetails(FanucMultiStrategy strategy, object configuration) : base(strategy,
        configuration)
    {
        if (!Configuration.ContainsKey("lines")) Configuration.Add("lines", 60);

        if (!Configuration.ContainsKey("map")) Configuration.Add("map", new Dictionary<object, object>());

        if (!Configuration.ContainsKey("extract"))
            Configuration.Add("extract", @"^\( *(?<key>[^\):\n]+[^ \):\n]) *: *(?<value>[^\):\n]+[^ \):\n])* *\)$");
    }

    public override async Task InitPathsAsync()
    {
        await Strategy.Apply(typeof(veneers.ProductionDataExternalSubprogramDetails), "production", true);
    }

    public override async Task CollectForEachPathAsync(short currentPath, string[] axis, string[] spindle,
        dynamic pathMarker)
    {
        await Strategy.SetNativeKeyed("program_numbers",
            await Strategy.Platform.RdPrgNumAsync());
        var currentProgramNumber = Strategy.GetKeyed("program_numbers")!
            .response.cnc_rdprgnum.prgnum.data;
        var mainProgramNumber = Strategy.GetKeyed("program_numbers")!
            .response.cnc_rdprgnum.prgnum.mdata;

        await Strategy.Peel("production",
            new[]
            {
                Strategy.GetKeyed("program_numbers"),
                await Strategy.SetNativeKeyed("pieces_produced",
                    await Strategy.Platform.RdParamDoubleWordNoAxisAsync(6711)),
                await Strategy.SetNativeKeyed("pieces_produced_life",
                    await Strategy.Platform.RdParamDoubleWordNoAxisAsync(6712)),
                await Strategy.SetNativeKeyed("pieces_remaining",
                    await Strategy.Platform.RdParamDoubleWordNoAxisAsync(6713)),
                await Strategy.SetNativeKeyed("cycle_time_min",
                    await Strategy.Platform.RdParamDoubleWordNoAxisAsync(6758)),
                await Strategy.SetNativeKeyed("cycle_time_ms",
                    await Strategy.Platform.RdParamDoubleWordNoAxisAsync(6757)),
                Strategy.GetKeyed("program_numbers+last")
            },
            new[]
            {
                Configuration["lines"],
                Configuration["map"],
                Configuration["extract"]
            });

        Strategy.SetKeyed("program_numbers+last",
            Strategy.GetKeyed("program_numbers"));
    }
}