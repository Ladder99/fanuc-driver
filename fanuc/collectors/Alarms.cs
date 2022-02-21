using System.Text.RegularExpressions;
using System.Threading.Tasks;
using l99.driver.fanuc.strategies;

namespace l99.driver.fanuc.collectors
{
    public class Alarms : FanucMultiStrategyCollector
    {
        public Alarms(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }

        private bool _warned = false;
        
        public override async Task InitPathsAsync()
        {
            await strategy.Apply(typeof(fanuc.veneers.AlarmsSeries), "alarms");
        }
        
        public override async Task CollectForEachPathAsync(short current_path, string[] axis, string[] spindle, dynamic path_marker)
        {
            var obsFocasSupport = strategy.GetKeyed($"obs+focas_support");

            if (obsFocasSupport == null)
            {
                if (!_warned)
                {
                    logger.Warn($"[{strategy.Machine.Id}] Machine info observation is required to correctly evaluate alarms.");
                    _warned = true;
                }
                
                await strategy.SetNativeKeyed($"alarms", 
                    await strategy.Platform.RdAlmMsg2Async(-1, 10));
            }
            else
            {
                if(Regex.IsMatch(string.Join("",obsFocasSupport),
                       "30i[A-Z]?|31i[A-Z]?|32i[A-Z]?|0i[D|F]|PMi[A]?"))
                {
                    await strategy.SetNativeKeyed($"alarms", 
                        await strategy.Platform.RdAlmMsg2Async(-1, 10));
                }
                else
                {
                    await strategy.SetNativeKeyed($"alarms", 
                        await strategy.Platform.RdAlmMsgAsync(-1, 10));
                }
            }

            var obsAlarms = await strategy.Peel("alarms", 
                strategy.GetKeyed($"alarms"),
                current_path,
                axis,
                obsFocasSupport,
                strategy.GetKeyed($"alarms+last"));

            strategy.SetKeyed($"alarms+last", 
                strategy.GetKeyed($"alarms"));
            
            strategy.SetKeyed($"obs+alarms", 
                obsAlarms.veneer.LastChangedValue.alarms);
        }
    }
}