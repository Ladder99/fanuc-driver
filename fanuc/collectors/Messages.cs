using System.Threading.Tasks;
using l99.driver.fanuc.strategies;

namespace l99.driver.fanuc.collectors
{
    public class Messages : FanucMultiStrategyCollector
    {
        public Messages(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }
        
        public override async Task InitPathsAsync()
        {
            await strategy.Apply(typeof(fanuc.veneers.OpMsgs), "messages");
        }
        
        public override async Task CollectForEachPathAsync(short current_path, dynamic path_marker)
        {
            await strategy.SetNativeAndPeel("messages", await strategy.Platform.RdOpMsgAsync(0, 6+256));
        }
    }
}