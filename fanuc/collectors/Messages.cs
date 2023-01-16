using l99.driver.fanuc.strategies;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors
{
    // ReSharper disable once UnusedType.Global
    public class Messages : FanucMultiStrategyCollector
    {
        public Messages(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }

        private bool _warned;
        
        public override async Task InitPathsAsync()
        {
            await Strategy.Apply(typeof(fanuc.veneers.OpMsgs), "messages");
        }
        
        public override async Task CollectForEachPathAsync(short currentPath, string[] axis, string[] spindle, dynamic pathMarker)
        {
            var obsFocasSupport = Strategy.GetKeyed($"obs+focas_support");
            
            short msgType = 0;
            short msgLength = 6 + 256;
            
            if (obsFocasSupport == null)
            {
                if (!_warned)
                {
                    Logger.Warn($"[{Strategy.Machine.Id}] Machine info observation is required to correctly evaluate operator messages.");
                    _warned = true;
                }
            }
            else
            {
                if (Regex.IsMatch(string.Join("", obsFocasSupport), "15"))
                {
                    msgType = -1;
                    msgLength = 578;
                }
                
                await Strategy.SetNativeKeyed($"messages",
                    await Strategy.Platform.RdOpMsgAsync(msgType, msgLength));
            
                await Strategy.Peel("messages", 
                    Strategy.GetKeyed($"messages"));
            }
            
        }
    }
}