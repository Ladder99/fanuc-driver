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
            await strategy.SetNative("program-name", 
                await strategy.Platform.ExePrgNameAsync());
            var o_num = strategy.Get("program-name").response.cnc_exeprgname.exeprg.o_num;
            
            await strategy.Peel("production",
                strategy.Get("program-name"),
                await strategy.SetNative("prog-dir", 
                    await strategy.Platform.RdProgDir3Async(o_num)),
                await strategy.SetNative("pieces-produced", 
                    await strategy.Platform.RdParamDoubleWordNoAxisAsync(6711)),
                await strategy.SetNative("pieces-produced-life", 
                    await strategy.Platform.RdParamDoubleWordNoAxisAsync(6712)),
                await strategy.SetNative("pieces-remaining", 
                    await strategy.Platform.RdParamDoubleWordNoAxisAsync(6713)),
                await strategy.SetNative("cycle-time-min", 
                    await strategy.Platform.RdParamDoubleWordNoAxisAsync(6758)),
                await strategy.SetNative("cycle-time-ms", 
                    await strategy.Platform.RdParamDoubleWordNoAxisAsync(6757)));
        }
    }
}