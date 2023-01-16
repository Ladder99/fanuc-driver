using l99.driver.fanuc.strategies;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors
{
    // ReSharper disable once UnusedType.Global
    public class Alarms : FanucMultiStrategyCollector
    {
        public Alarms(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }

        private bool _warned;
        
        public override async Task InitPathsAsync()
        {
            await Strategy.Apply(typeof(fanuc.veneers.AlarmsSeries), "alarms");
        }
        
        public override async Task CollectForEachPathAsync(short currentPath, string[] axis, string[] spindle, dynamic pathMarker)
        {
            var obsFocasSupport = Strategy.GetKeyed($"obs+focas_support");

            if (obsFocasSupport == null)
            {
                if (!_warned)
                {
                    Logger.Warn($"[{Strategy.Machine.Id}] Machine info observation is required to correctly evaluate alarms.");
                    _warned = true;
                }
                
                await Strategy.SetNativeKeyed($"alarms", 
                    await Strategy.Platform.RdAlmMsg2Async(-1, 10));
            }
            else
            {
                if(Regex.IsMatch(string.Join("",obsFocasSupport),
                       "30i[A-Z]?|31i[A-Z]?|32i[A-Z]?|0i[D|F]|PMi[A]?"))
                {
                    await Strategy.SetNativeKeyed($"alarms", 
                        await Strategy.Platform.RdAlmMsg2Async(-1, 10));
                }
                else
                {
                    await Strategy.SetNativeKeyed($"alarms", 
                        await Strategy.Platform.RdAlmMsgAsync(-1, 10));
                }
            }

            var obsAlarms = await Strategy.Peel("alarms", 
                Strategy.GetKeyed($"alarms"),
                currentPath,
                axis,
                obsFocasSupport,
                Strategy.GetKeyed($"alarms+last"));

            Strategy.SetKeyed($"alarms+last", 
                Strategy.GetKeyed($"alarms"));
            
            Strategy.SetKeyed($"obs+alarms", 
                obsAlarms.veneer.LastChangedValue.alarms);
        }
    }
}