using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.collectors
{
    public class GCodeData : FanucCollector2
    {
        public GCodeData(Machine machine, object cfg) : base(machine, cfg)
        {
            
        }
        
        public override async Task InitPathsAsync()
        {
            await Apply(typeof(fanuc.veneers.GCodeBlocks), "gcode_data");
        }
        
        public override async Task CollectForEachPathAsync(short current_path, dynamic path_marker)
        {
            await Peel("gcode_data",
                await SetNative("blkcount", await platform.RdBlkCountAsync()),
                await SetNative("actpt", await platform.RdActPtAsync()),
                await SetNative("execprog", await platform.RdExecProgAsync(256)));
        }
    }
}