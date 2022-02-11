using System.Threading.Tasks;
using l99.driver.fanuc.strategies;

namespace l99.driver.fanuc.collectors
{
    public class Alarms : FanucMultiStrategyCollector
    {
        public Alarms(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }
        
        public override async Task InitPathsAsync()
        {
            await strategy.Apply(typeof(fanuc.veneers.Alarms2), "alarms");
        }
        
        public override async Task CollectForEachPathAsync(short current_path, dynamic path_marker)
        {
            await strategy.SetNativeAndPeel("alarms", await strategy.Platform.RdAlmMsg2Async(-1, 10));
        }
    }
}