using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.collectors
{
    public class ThreeWay : FanucCollector2
    {
        public ThreeWay(Machine machine, object cfg) : base(machine, cfg)
        {
            
        }
        
        public override async Task InitRootAsync()
        {
            
        }
        
        public override async Task InitPathsAsync()
        {
            await Apply(typeof(fanuc.veneers.ThreeWayStateData), "state", isCompound: true);
            
            await Apply(typeof(fanuc.veneers.ThreeWayProductionData), "production", isCompound: true);
            
            await Apply(typeof(fanuc.veneers.ThreeWayAlarmData), "alarms");
        }
        
        public override async Task InitAxisAndSpindleAsync()
        {
            
        }
        
        public override async Task<bool> CollectBeginAsync()
        {
            return await base.CollectBeginAsync();
        }
        
        public override async Task CollectRootAsync()
        {
            await SetNative("poweron-time-min", await platform.RdParamDoubleWordNoAxisAsync(6750));
            
            await SetNative("operating-time-min", await platform.RdParamDoubleWordNoAxisAsync(6752));
            
            await SetNative("cutting-time-min", await platform.RdParamDoubleWordNoAxisAsync(6754));
        }

        public override async Task CollectForEachPathAsync(short current_path, dynamic path_marker)
        {
            await Peel("state",
                await SetNative("stat-info", await platform.StatInfoAsync()),
                Get("poweron-time-min"),
                Get("operating-time-min"),
                Get("cutting-time-min"));
            
            await Peel("production",
                await SetNative("feed-override", await platform.RdPmcRngGByteAsync(12)),
                await SetNative("rapid-override", await platform.RdPmcRngGByteAsync(14)),
                await SetNative("spindle-override", await platform.RdPmcRngGByteAsync(30)),
                await SetNative("program-name", await platform.ExePrgNameAsync()),
                await SetNative("pieces-produced", await platform.RdParamDoubleWordNoAxisAsync(6711)),
                await SetNative("pieces-produced-life", await platform.RdParamDoubleWordNoAxisAsync(6712)),
                await SetNative("pieces-remaining", await platform.RdParamDoubleWordNoAxisAsync(6713)),
                await SetNative("cycle-time-min", await platform.RdParamDoubleWordNoAxisAsync(6758)),
                await SetNative("cycle-time-ms", await platform.RdParamDoubleWordNoAxisAsync(6757)));
            
            await Peel("alarms", 
                await SetNative("alarm", await platform.RdAlmMsgAllAsync(10,20)),
                await SetNative("message", await platform.RdOpMsgAsync(0, 6+256)));
        }

        public override async Task CollectForEachAxisAsync(short current_axis, string axis_name, dynamic axis_split, dynamic axis_marker)
        {
            
        }

        public override async Task CollectForEachSpindleAsync(short current_spindle, string spindle_name, dynamic spindle_split, dynamic spindle_marker)
        {
            
        }

        public override async Task CollectEndAsync()
        {
            await base.CollectEndAsync();
        }
    }
}