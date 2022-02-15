using System.Threading.Tasks;
using l99.driver.fanuc.strategies;

namespace l99.driver.fanuc.collectors
{
    public class AxisData : FanucMultiStrategyCollector
    {
        public AxisData(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }

        private bool _warned = false;
        
        public override async Task InitPathsAsync()
        {
            await strategy.Apply(typeof(fanuc.veneers.Figures), "figures");
        }

        public override async Task InitAxisAndSpindleAsync()
        {
            await strategy.Apply(typeof(fanuc.veneers.AxisData), "axis", isCompound: true );
        }
        
        public override async Task CollectForEachPathAsync(short current_path, dynamic path_marker)
        {
            await strategy.SetNative($"figures+{current_path}", 
                await strategy.Platform.GetFigureAsync(0, 32));
            
            await strategy.SetNative($"axis_load+{current_path}", 
                await strategy.Platform.RdSvMeterAsync());
        }

        public override async Task CollectForEachAxisAsync(short current_axis, string axis_name, dynamic axis_split, dynamic axis_marker)
        {
            var axis_key = string.Join("/", axis_split);
            var path_key = strategy.Get("current_path");
            
            var obs_focas_support = strategy.Get($"obs+focas_support+{path_key}");
            var obs_alarms = strategy.Get($"obs+alarms+{path_key}");

            if (obs_alarms == null || obs_focas_support == null)
            {
                if (!_warned)
                {
                    logger.Warn($"[{strategy.Machine.Id}] Machine info and alarms observations are required to correctly evaluate axis data.");
                    _warned = true;
                }
            }
            
            // 200-206                  servo, coder status

            // 300                      servo error

            // 308 byte                 motor temperature (c)
            await strategy.SetNative($"diag_servo_temp+{axis_key}", 
                await strategy.Platform.DiagnossByteAsync(308, current_axis));
            
            // 309 byte                 coder temperature (c)
            await strategy.SetNative($"diag_coder_temp+{axis_key}", 
                await strategy.Platform.DiagnossByteAsync(309, current_axis));
            
            // 4901 dword               servo power consumption (watt)
            await strategy.SetNative($"diag_power+{axis_key}", 
                await strategy.Platform.DiagnossDoubleWordAsync(4901, current_axis));
            
            await strategy.SetNative($"axis_dynamic+{axis_key}", 
                await strategy.Platform.RdDynamic2Async(current_axis, 44, 2));

            await strategy.Peel("axis",
                current_axis,
                strategy.Get($"axis_names"),
                strategy.Get($"axis_dynamic+{axis_key}"), 
                strategy.Get($"figures+{path_key}"),
                strategy.Get($"axis_load+{path_key}"),
                strategy.Get($"diag_servo_temp+{axis_key}"),
                strategy.Get($"diag_coder_temp+{axis_key}"),
                strategy.Get($"diag_power+{axis_key}"),
                obs_focas_support,
                obs_alarms,
                strategy.Get($"axis_dynamic+last+{axis_key}"));
            
            strategy.Set($"axis_dynamic+last+{axis_key}", 
                strategy.Get($"axis_dynamic+{axis_key}"));
        }
    }
}