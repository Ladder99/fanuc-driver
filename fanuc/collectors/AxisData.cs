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
        
        public override async Task CollectForEachPathAsync(short current_path, string[] axis, string[] spindle, dynamic path_marker)
        {
            await strategy.SetNativeKeyed($"figures", 
                await strategy.Platform.GetFigureAsync(0, 32));
            
            await strategy.SetNativeKeyed($"axis_load", 
                await strategy.Platform.RdSvMeterAsync());
        }

        public override async Task CollectForEachAxisAsync(short current_path, short current_axis, string axis_name, dynamic axis_split, dynamic axis_marker)
        {
            var obs_focas_support = strategy.Get($"obs+focas_support+{current_path}");
            var obs_alarms = strategy.Get($"obs+alarms+{current_path}");

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
            await strategy.SetNativeKeyed($"diag_servo_temp", 
                await strategy.Platform.DiagnossByteAsync(308, current_axis));
            
            // 309 byte                 coder temperature (c)
            await strategy.SetNativeKeyed($"diag_coder_temp", 
                await strategy.Platform.DiagnossByteAsync(309, current_axis));
            
            // 4901 dword               servo power consumption (watt)
            await strategy.SetNativeKeyed($"diag_power", 
                await strategy.Platform.DiagnossDoubleWordAsync(4901, current_axis));
            
            await strategy.SetNativeKeyed($"axis_dynamic", 
                await strategy.Platform.RdDynamic2Async(current_axis, 44, 2));

            await strategy.Peel("axis",
                current_axis,
                strategy.Get($"axis_names+{current_path}"),
                strategy.GetKeyed($"axis_dynamic"), 
                strategy.Get($"figures+{current_path}"),
                strategy.Get($"axis_load+{current_path}"),
                strategy.GetKeyed($"diag_servo_temp"),
                strategy.GetKeyed($"diag_coder_temp"),
                strategy.GetKeyed($"diag_power"),
                obs_focas_support,
                obs_alarms,
                strategy.GetKeyed($"axis_dynamic+last"));
            
            strategy.SetKeyed($"axis_dynamic+last", 
                strategy.GetKeyed($"axis_dynamic"));
        }
    }
}