using l99.driver.fanuc.strategies;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors
{
    // ReSharper disable once UnusedType.Global
    public class GCodeData : FanucMultiStrategyCollector
    {
        public GCodeData(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }
        
        public override async Task InitPathsAsync()
        {
            await Strategy.Apply(typeof(fanuc.veneers.GCodeBlocks), "gcode");
        }
        
        public override async Task CollectForEachPathAsync(short currentPath, string[] axis, string[] spindle, dynamic pathMarker)
        {
            //TODO: make configurable
            /*
            await strategy.SetNativeKeyed($"blkcount", 
                    await strategy.Platform.RdBlkCountAsync())
            */
            
            await Strategy.Peel("gcode",
                null,
                await Strategy.SetNativeKeyed($"actpt", 
                    await Strategy.Platform.RdActPtAsync()),
                await Strategy.SetNativeKeyed($"execprog", 
                    await Strategy.Platform.RdExecProgAsync(256)));
        }
    }
}