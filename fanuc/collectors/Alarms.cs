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
            var obs_focas_support = strategy.Get($"obs+focas_support+{current_path}");

            if (obs_focas_support == null)
            {
                if (!_warned)
                {
                    logger.Warn($"[{strategy.Machine.Id}] Machine info observation is required to correctly evaluate alarms.");
                    _warned = true;
                }
                
                await strategy.SetNative($"alarms+{current_path}", 
                    await strategy.Platform.RdAlmMsg2Async(-1, 10));
            }
            else
            {
                if(Regex.IsMatch(string.Join("",obs_focas_support),
                       "30i[A-Z]?|31i[A-Z]?|32i[A-Z]?|0i[D|F]|PMi[A]?"))
                {
                    await strategy.SetNative($"alarms+{current_path}", 
                        await strategy.Platform.RdAlmMsg2Async(-1, 10));
                }
                else
                {
                    await strategy.SetNative($"alarms+{current_path}", 
                        await strategy.Platform.RdAlmMsgAsync(-1, 10));
                }
            }

            var obs_alarms = await strategy.Peel("alarms", 
                strategy.Get($"alarms+{current_path}"),
                current_path,
                axis,
                obs_focas_support);

            strategy.Set($"obs+alarms+{current_path}", 
                obs_alarms.veneer.LastChangedValue.alarms);
        }
    }
}