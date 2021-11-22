using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.collectors
{
    public class Basic08 : FanucCollector2
    {
        public Basic08(Machine machine, object cfg) : base(machine, cfg)
        {
            
        }
        
        // global machine observations
        public override async Task InitRootAsync()
        {
            await Apply(typeof(fanuc.veneers.CNCId), "cnc_id");
            
            await Apply(typeof(fanuc.veneers.RdParamLData), "power_on_time");
        }
        
        // execution path observations
        public override async Task InitPathsAsync()
        {
            await Apply(typeof(fanuc.veneers.SysInfo), "sys_info");
            
            await Apply(typeof(fanuc.veneers.StatInfo), "stat_info");

            await Apply(typeof(fanuc.veneers.Figures), "figures");
            
            await Apply(typeof(fanuc.veneers.GCodeBlocks), "gcode_blocks");
        }
        
        // axis and spindle observations
        public override async Task InitAxisAndSpindleAsync()
        {
            await Apply(typeof(fanuc.veneers.RdDynamic2_1), "axis_data");
            
            await Apply(typeof(fanuc.veneers.RdActs2), "spindle_data");
        }
        
        // 
        //    collection sweep
        //
        //    begin => 
        //        root/global =>
        //        walk each execution path =>
        //        walk each axis in execution path =>
        //        walk each spindle in execution path =>
        //    end => 
        //    sleep => 
        //    begin ...
        // 
        
        public override async Task<bool> CollectBeginAsync()
        {
            return await base.CollectBeginAsync();
        }
        
        // reveal global machine observations
        public override async Task CollectRootAsync()
        {
            // single data point observation
            //
            //    set_native_and_peel
            //        1. cache focas returned value as "cnc_id"
            //        2. reveal observation bound by "cnc_id" in InitRootAsync function
            //
            await SetNativeAndPeel("cnc_id", await platform.CNCIdAsync());
                    
            await SetNativeAndPeel("power_on_time", await platform.RdParamDoubleWordNoAxisAsync(6750));
        }

        public override async Task CollectForEachPathAsync(short current_path, dynamic path_marker)
        {
            await SetNativeAndPeel("sys_info", await platform.SysInfoAsync());
                        
            await SetNativeAndPeel("stat_info", await platform.StatInfoAsync());
            
            await SetNativeAndPeel("figures", await platform.GetFigureAsync(0, 32));
            
            // compound observation
            //
            //    set_native
            //        1. cache focas returned value as "blkcount", "actpt", "execprog"
            //  
            //    peel
            //        1. reveal observation bound by "gcode_blocks" in InitPathAsync function
            //              "blkcount", "actpt", and "execprog" data is fed into the transformation logic
            //
            await Peel("gcode_blocks",
                await SetNative("blkcount", await platform.RdBlkCountAsync()),
                await SetNative("actpt", await platform.RdActPtAsync()),
                await SetNative("execprog", await platform.RdExecProgAsync(128)));
        }

        // reveal axis observations
        public override async Task CollectForEachAxisAsync(short current_axis, string axis_name, dynamic axis_split, dynamic axis_marker)
        {
            // 
            //  get
            //      retrieve "figures" value from cache previously set in CollectForEachPathAsync
            //
            await Peel("axis_data",
                await SetNative("axis_dynamic", await platform.RdDynamic2Async(current_axis, 44, 2)), 
                Get("figures"), 
                current_axis - 1);
        }

        // reveal spindle observations
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