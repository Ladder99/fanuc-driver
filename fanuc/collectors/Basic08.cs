using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.collectors
{
    public class Basic08 : FanucCollector2
    {
        public Basic08(Machine machine, int sweepMs = 1000, params dynamic[] additionalParams) : base(machine, sweepMs, additionalParams)
        {
            
        }
        
        public override async Task InitRootAsync()
        {
            await Apply(typeof(fanuc.veneers.CNCId), "cnc_id");
            
            await Apply(typeof(fanuc.veneers.RdParamLData), "power_on_time");
        }
        
        public override async Task InitPathsAsync()
        {
            await Apply(typeof(fanuc.veneers.SysInfo), "sys_info");
            
            await Apply(typeof(fanuc.veneers.StatInfoText), "stat_info");

            await Apply(typeof(fanuc.veneers.Figures), "figures");
            
            await Apply(typeof(fanuc.veneers.GCodeBlocks), "gcode_blocks");
        }
        
        public override async Task InitAxisAndSpindleAsync()
        {
            await Apply(typeof(fanuc.veneers.RdDynamic2_1), "axis_data");
            
            await Apply(typeof(fanuc.veneers.RdActs2), "spindle_data");
        }
        
        public override async Task<bool> CollectBeginAsync()
        {
            return await base.CollectBeginAsync();
        }
        
        public override async Task CollectRootAsync()
        {
            await SetNativeAndPeel("cnc_id", await platform.CNCIdAsync());
                    
            await SetNativeAndPeel("power_on_time", await platform.RdParamDoubleWordNoAxisAsync(6750));
        }

        public override async Task CollectForEachPathAsync(short current_path, dynamic path_marker)
        {
            await SetNativeAndPeel("sys_info", await platform.SysInfoAsync());
                        
            await SetNativeAndPeel("stat_info", await platform.StatInfoAsync());
            
            await SetNativeAndPeel("figures", await platform.GetFigureAsync(0, 32));
            
            await Peel("gcode_blocks",
                await SetNative("blkcount", await platform.RdBlkCountAsync()),
                await SetNative("actpt", await platform.RdActPtAsync()),
                await SetNative("execprog", await platform.RdExecProgAsync(128)));
        }

        public override async Task CollectForEachAxisAsync(short current_axis, string axis_name, dynamic axis_split, dynamic axis_marker)
        {
            await Peel("axis_data",
                await SetNative("axis_dynamic", await platform.RdDynamic2Async(current_axis, 44, 2)), 
                Get("figures"), 
                current_axis - 1);
        }

        public override async Task CollectForEachSpindleAsync(short current_spindle, string spindle_name, dynamic spindle_split, dynamic spindle_marker)
        {
            await SetNativeAndPeel("spindle_data", await platform.Acts2Async(current_spindle));
        }

        public override async Task CollectEndAsync()
        {
            await base.CollectEndAsync();
        }
    }
}