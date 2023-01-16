using l99.driver.fanuc.strategies;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors
{
    // ReSharper disable once UnusedType.Global
    public class AxisData : FanucMultiStrategyCollector
    {
        public AxisData(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }

        private bool _warned;
        
        public override async Task InitPathsAsync()
        {
            await Strategy.Apply(typeof(fanuc.veneers.Figures), "figures");
        }

        public override async Task InitAxisAsync()
        {
            await Strategy.Apply(typeof(fanuc.veneers.AxisData), "axis", isCompound: true );
        }
        
        public override async Task CollectForEachPathAsync(short currentPath, string[] axis, string[] spindle, dynamic pathMarker)
        {
            await Strategy.SetNativeKeyed($"figures", 
                await Strategy.Platform.GetFigureAsync(0, 32));
            
            await Strategy.SetNativeKeyed($"axis_load", 
                await Strategy.Platform.RdSvMeterAsync());
        }

        public override async Task CollectForEachAxisAsync(short currentPath, short currentAxis, string axisName, dynamic axisSplit, dynamic axisMarker)
        {
            var obs_focas_support = Strategy.Get($"obs+focas_support+{currentPath}");
            var obs_alarms = Strategy.Get($"obs+alarms+{currentPath}");

            if (obs_alarms == null || obs_focas_support == null)
            {
                if (!_warned)
                {
                    Logger.Warn($"[{Strategy.Machine.Id}] Machine info and alarms observations are required to correctly evaluate axis data.");
                    _warned = true;
                }
            }
            
            // 200-206                  servo, coder status

            // 300                      servo position error

            // 308 byte                 motor temperature (c)
            await Strategy.SetNativeKeyed($"diag_servo_temp", 
                await Strategy.Platform.DiagnossByteAsync(308, currentAxis));
            
            // 309 byte                 coder temperature (c)
            await Strategy.SetNativeKeyed($"diag_coder_temp", 
                await Strategy.Platform.DiagnossByteAsync(309, currentAxis));
            
            // 4901 dword               servo power consumption (watt)
            await Strategy.SetNativeKeyed($"diag_power", 
                await Strategy.Platform.DiagnossDoubleWordAsync(4901, currentAxis));
            
            await Strategy.SetNativeKeyed($"axis_dynamic", 
                await Strategy.Platform.RdDynamic2Async(currentAxis, 44, 2));

            await Strategy.Peel("axis",
                currentAxis,
                Strategy.Get($"axis_names+{currentPath}"),
                Strategy.GetKeyed($"axis_dynamic"), 
                Strategy.Get($"figures+{currentPath}"),
                Strategy.Get($"axis_load+{currentPath}"),
                Strategy.GetKeyed($"diag_servo_temp"),
                Strategy.GetKeyed($"diag_coder_temp"),
                Strategy.GetKeyed($"diag_power"),
                obs_focas_support,
                obs_alarms,
                Strategy.GetKeyed($"axis_dynamic+last"));
            
            Strategy.SetKeyed($"axis_dynamic+last", 
                Strategy.GetKeyed($"axis_dynamic"));
        }
    }
}