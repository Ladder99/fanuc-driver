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
        
        public override async Task CollectForEachPathAsync(short current_path, dynamic path_marker)
        {
            await strategy.SetNative($"program_name+{current_path}", 
                await strategy.Platform.ExePrgNameAsync());
            var o_num = strategy.Get($"program_name+{current_path}")
                .response.cnc_exeprgname.exeprg.o_num;
            
            await strategy.Peel("production",
                strategy.Get($"program_name+{current_path}"),
                await strategy.SetNative($"prog_dir+{current_path}", 
                    await strategy.Platform.RdProgDir3Async(o_num)),
                await strategy.SetNative($"pieces_produced+{current_path}", 
                    await strategy.Platform.RdParamDoubleWordNoAxisAsync(6711)),
                await strategy.SetNative($"pieces_produced_life+{current_path}", 
                    await strategy.Platform.RdParamDoubleWordNoAxisAsync(6712)),
                await strategy.SetNative($"pieces_remaining+{current_path}", 
                    await strategy.Platform.RdParamDoubleWordNoAxisAsync(6713)),
                await strategy.SetNative($"cycle_time_min+{current_path}", 
                    await strategy.Platform.RdParamDoubleWordNoAxisAsync(6758)),
                await strategy.SetNative($"cycle_time_ms+{current_path}", 
                    await strategy.Platform.RdParamDoubleWordNoAxisAsync(6757)));
        }
    }
}