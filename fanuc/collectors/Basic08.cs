using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.collectors
{
    public class Basic08 : FanucCollector2
    {
        public Basic08(Machine machine, int sweepMs = 1000) : base(machine, sweepMs)
        {
            
        }
        
        public override async Task InitRootAsync()
        {
            apply(typeof(fanuc.veneers.CNCId), "cnc_id");
            
            apply(typeof(fanuc.veneers.RdParamLData), "power_on_time");
        }
        
        public override async Task InitPathsAsync()
        {
            apply(typeof(fanuc.veneers.SysInfo), "sys_info");
            
            apply(typeof(fanuc.veneers.StatInfo), "stat_info");

            apply(typeof(fanuc.veneers.Figures), "figures");
            
            apply(typeof(fanuc.veneers.GCodeBlocks), "gcode_blocks");
        }
        
        public override async Task InitAxisAndSpindleAsync()
        {
            apply(typeof(fanuc.veneers.RdDynamic2_1), "axis_data");
            
            apply(typeof(fanuc.veneers.RdActs2), "spindle_data");
        }
        
        public async Task<bool> CollectBeginAsync()
        {
            return await base.CollectBeginAsync();
        }
        
        public async Task CollectRootAsync()
        {
            await set_native_and_peel("cnc_id", await _platform.CNCIdAsync());
                    
            await set_native_and_peel("power_on_time", await _platform.RdParamDoubleWordNoAxisAsync(6750));
        }

        public async Task CollectForEachPathAsync(short current_path, dynamic path_marker)
        {
            await set_native_and_peel("sys_info", await _platform.SysInfoAsync());
                        
            await set_native_and_peel("stat_info", await _platform.StatInfoAsync());
            
            await set_native_and_peel("figures", await _platform.GetFigureAsync(0, 32));
            
            await peel("gcode_blocks",
                await set_native("blkcount", await _platform.RdBlkCountAsync()),
                await set_native("actpt", await _platform.RdActPtAsync()),
                await set_native("execprog", await _platform.RdExecProgAsync(128)));
        }

        public async Task CollectForEachAxisAsync(short current_axis, dynamic axis_split, dynamic axis_marker)
        {
            await peel("axis_data",
                await set_native("axis_dynamic", await _platform.RdDynamic2Async(current_axis, 44, 2)), 
                get("figures"), 
                current_axis - 1);
        }

        public async Task CollectForEachSpindleAsync(short current_spindle, dynamic spindle_split, dynamic spindle_marker)
        {
            await set_native_and_peel("spindle_data", await _platform.Acts2Async(current_spindle));
        }

        public async Task CollectEndAsync()
        {
            await base.CollectEndAsync();
        }
    }
}