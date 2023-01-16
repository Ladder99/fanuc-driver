using l99.driver.fanuc.strategies;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors
{
    // ReSharper disable once UnusedType.Global
    public class ProductionData : FanucMultiStrategyCollector
    {
        public ProductionData(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }
        
        public override async Task InitPathsAsync()
        {
            await Strategy.Apply(typeof(fanuc.veneers.ProductionData), "production", isCompound: true);
        }
        
        public override async Task CollectForEachPathAsync(short currentPath, string[] axis, string[] spindle, dynamic pathMarker)
        {
            // cnc_rdprgnum
            await Strategy.SetNativeKeyed($"program_numbers",
                await Strategy.Platform.RdPrgNumAsync());

            var currentProgramNumber = Strategy.GetKeyed($"program_numbers")!
                .response.cnc_rdprgnum.prgnum.data;
            
            var mainProgramNumber = Strategy.GetKeyed($"program_numbers")!
                .response.cnc_rdprgnum.prgnum.mdata;

            await Strategy.Peel("production",
                Strategy.GetKeyed($"program_numbers"),
                await Strategy.SetNativeKeyed($"current_program_info", 
                    await Strategy.Platform.RdProgDir3Async(currentProgramNumber)),
                await Strategy.SetNativeKeyed($"main_program_info", 
                    await Strategy.Platform.RdProgDir3Async(mainProgramNumber)),
                await Strategy.SetNativeKeyed($"pieces_produced", 
                    await Strategy.Platform.RdParamDoubleWordNoAxisAsync(6711)),
                await Strategy.SetNativeKeyed($"pieces_produced_life", 
                    await Strategy.Platform.RdParamDoubleWordNoAxisAsync(6712)),
                await Strategy.SetNativeKeyed($"pieces_remaining", 
                    await Strategy.Platform.RdParamDoubleWordNoAxisAsync(6713)),
                await Strategy.SetNativeKeyed($"cycle_time_min", 
                    await Strategy.Platform.RdParamDoubleWordNoAxisAsync(6758)),
                await Strategy.SetNativeKeyed($"cycle_time_ms", 
                    await Strategy.Platform.RdParamDoubleWordNoAxisAsync(6757))); 
        }
    }
}