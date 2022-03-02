using System.Collections.Generic;
using System.Threading.Tasks;
using l99.driver.fanuc.strategies;

namespace l99.driver.fanuc.collectors
{
    public class ToolData : FanucMultiStrategyCollector
    {
        public ToolData(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }

        public override async Task InitPathsAsync()
        {
            await strategy.Apply(typeof(veneers.ToolData), "tool");
        }
        
        public override async Task CollectForEachPathAsync(short current_path, string[] axis, string[] spindle, dynamic path_marker)
        {
            await strategy.Peel("tool",
                await strategy.SetNativeKeyed($"modal_t", 
                    await strategy.Platform.ModalAsync(108,0,3)),
                await strategy.SetNativeKeyed($"toolnum", 
                    await strategy.Platform.ToolNumAsync()));
        }
    }
}