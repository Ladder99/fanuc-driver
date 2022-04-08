using l99.driver.fanuc.strategies;

namespace l99.driver.fanuc.collectors
{
    public class Messages : FanucMultiStrategyCollector
    {
        public Messages(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }

        private bool _warned = false;
        
        public override async Task InitPathsAsync()
        {
            await strategy.Apply(typeof(fanuc.veneers.OpMsgs), "messages");
        }
        
        public override async Task CollectForEachPathAsync(short current_path, string[] axis, string[] spindle, dynamic path_marker)
        {
            var obs_focas_support = strategy.GetKeyed($"obs+focas_support");
            
            short msg_type = 0;
            short msg_length = 6 + 256;
            
            if (obs_focas_support == null)
            {
                if (!_warned)
                {
                    logger.Warn($"[{strategy.Machine.Id}] Machine info observation is required to correctly evaluate operator messages.");
                    _warned = true;
                }
            }
            else
            {
                if (Regex.IsMatch(string.Join("", obs_focas_support), "15"))
                {
                    msg_type = -1;
                    msg_length = 578;
                }
                
                await strategy.SetNativeKeyed($"messages",
                    await strategy.Platform.RdOpMsgAsync(msg_type, msg_length));
            
                await strategy.Peel("messages", 
                    strategy.GetKeyed($"messages"));
            }
            
        }
    }
}