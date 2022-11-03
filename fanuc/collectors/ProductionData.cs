using l99.driver.fanuc.strategies;

namespace l99.driver.fanuc.collectors
{
    public class ProductionData : FanucMultiStrategyCollector
    {
        public ProductionData(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }
        
        public override async Task InitPathsAsync()
        {
            await strategy.Apply(typeof(fanuc.veneers.ProductionData), "production", isCompound: true);
        }
        
        public override async Task CollectForEachPathAsync(short current_path, string[] axis, string[] spindle, dynamic path_marker)
        {
            // cnc_rdprgnum
            await strategy.SetNativeKeyed($"program_numbers",
                await strategy.Platform.RdPrgNumAsync());

            var current_program_number = strategy.GetKeyed($"program_numbers")
                .response.cnc_rdprgnum.prgnum.data;
            
            var main_program_number = strategy.GetKeyed($"program_numbers")
                .response.cnc_rdprgnum.prgnum.mdata;

            await strategy.Peel("production",
                strategy.GetKeyed($"program_numbers"),
                await strategy.SetNativeKeyed($"current_program_info", 
                    await strategy.Platform.RdProgDir3Async(current_program_number)),
                await strategy.SetNativeKeyed($"main_program_info", 
                    await strategy.Platform.RdProgDir3Async(main_program_number)),
                await strategy.SetNativeKeyed($"pieces_produced", 
                    await strategy.Platform.RdParamDoubleWordNoAxisAsync(6711)),
                await strategy.SetNativeKeyed($"pieces_produced_life", 
                    await strategy.Platform.RdParamDoubleWordNoAxisAsync(6712)),
                await strategy.SetNativeKeyed($"pieces_remaining", 
                    await strategy.Platform.RdParamDoubleWordNoAxisAsync(6713)),
                await strategy.SetNativeKeyed($"cycle_time_min", 
                    await strategy.Platform.RdParamDoubleWordNoAxisAsync(6758)),
                await strategy.SetNativeKeyed($"cycle_time_ms", 
                    await strategy.Platform.RdParamDoubleWordNoAxisAsync(6757))); 
        }
    }
}