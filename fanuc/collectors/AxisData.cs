using System.Threading.Tasks;
using l99.driver.fanuc.strategies;

namespace l99.driver.fanuc.collectors
{
    public class AxisData : FanucMultiStrategyCollector
    {
        public AxisData(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }

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
            await strategy.SetNative("figures", await strategy.Platform.GetFigureAsync(0, 32));
            
            await strategy.SetNative("axis_load", await strategy.Platform.RdSvMeterAsync());
        }

        public override async Task CollectForEachAxisAsync(short current_axis, string axis_name, dynamic axis_split, dynamic axis_marker)
        {
            // 200-206                  servo, coder status
            // TODO
            
            // 300                      servo error

            // 308 byte                 motor temperature (c)
            await strategy.SetNative("diag_servo_temp", await strategy.Platform.DiagnossByteAsync(308, current_axis));
            
            // 309 byte                 coder temperature (c)
            await strategy.SetNative("diag_coder_temp", await strategy.Platform.DiagnossByteAsync(309, current_axis));
            
            // 4901 dword               servo power consumption (watt)
            await strategy.SetNative("diag_power", await strategy.Platform.DiagnossDoubleWordAsync(4901, current_axis));
            
            await strategy.SetNative("axis_dynamic", await strategy.Platform.RdDynamic2Async(current_axis, 44, 2));
            
            await strategy.Peel("axis",
                current_axis,
                strategy.Get("axis_names"),
                strategy.Get("axis_dynamic"), 
                strategy.Get("figures"),
                strategy.Get("axis_load"),
                strategy.Get("diag_servo_temp"),
                strategy.Get("diag_coder_temp"),
                strategy.Get("diag_power"));
        }
    }
}