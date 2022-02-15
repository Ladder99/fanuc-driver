using System.Threading.Tasks;
using l99.driver.fanuc.strategies;

namespace l99.driver.fanuc.collectors
{
    public class MachineInfo : FanucMultiStrategyCollector
    {
        public MachineInfo(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }
        
        public override async Task InitPathsAsync()
        {
            await strategy.Apply(typeof(fanuc.veneers.SysInfo), "machine");
        }
        
        public override async Task CollectForEachPathAsync(short current_path, string[] axis, string[] spindle, dynamic path_marker)
        {
            await strategy.SetNative($"machine+{current_path}",
                await strategy.Platform.SysInfoAsync());
            
            var obs_machine = await strategy.Peel($"machine", 
                strategy.Get($"machine+{current_path}"));

            strategy.Set($"obs+focas_support+{current_path}", 
                obs_machine.veneer.LastArrivedValue.focas_support);
        }
    }
}