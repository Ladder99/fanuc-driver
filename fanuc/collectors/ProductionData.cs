using System.Threading.Tasks;
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
            await strategy.SetNativeKeyed($"program_name", 
                await strategy.Platform.ExePrgNameAsync());
            var o_num = strategy.GetKeyed($"program_name")
                .response.cnc_exeprgname.exeprg.o_num;
            
            await strategy.Peel("production",
                strategy.GetKeyed($"program_name"),
                await strategy.SetNativeKeyed($"prog_dir", 
                    await strategy.Platform.RdProgDir3Async(o_num)),
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