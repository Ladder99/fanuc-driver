using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.collectors
{
    public class ProgramAndPieceCount : FanucCollector2
    {
        public ProgramAndPieceCount(Machine machine, object cfg) : base(machine, cfg)
        {
            
        }
        
        public override async Task InitRootAsync()
        {
            await Apply(typeof(fanuc.veneers.Alarms), "alarms");
            
            await Apply(typeof(fanuc.veneers.OpMsgs), "message");
        }
        
        public override async Task InitPathsAsync()
        {
            await Apply(typeof(fanuc.veneers.StatInfo), "stat-info");
            
            await Apply(typeof(fanuc.veneers.RdPmcRngByte), "feedrate-override");
            
            await Apply(typeof(fanuc.veneers.RdPmcRngByte), "feedrate-rapid-override");
            
            await Apply(typeof(fanuc.veneers.RdPmcRngByte), "spindle-override");
            
            await Apply(typeof(fanuc.veneers.PrgName), "program-name");
            
            await Apply(typeof(fanuc.veneers.RdParamLData), "pieces-produced");
            
            await Apply(typeof(fanuc.veneers.RdParamLData), "pieces-produced-life");
            
            await Apply(typeof(fanuc.veneers.RdParamLData), "pieces-remaining");
            
            await Apply(typeof(fanuc.veneers.RdParamLData), "cycle-time");
            
            await Apply(typeof(fanuc.veneers.RdParamLData), "cycle-time-ms");
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
            await SetNativeAndPeel("alarms", await platform.RdAlmMsgAllAsync(10,20));
                    
            await SetNativeAndPeel("message", await platform.RdOpMsgAsync(0, 6+256));
        }

        public override async Task CollectForEachPathAsync(short current_path, dynamic path_marker)
        {
            await SetNativeAndPeel("stat-info", await platform.StatInfoAsync());
            
            await SetNativeAndPeel("feedrate-override", await platform.RdPmcRngGByteAsync(12));
            
            await SetNativeAndPeel("feedrate-rapid-override", await platform.RdPmcRngGByteAsync(14));
            
            await SetNativeAndPeel("spindle-override", await platform.RdPmcRngGByteAsync(30));
            
            await SetNativeAndPeel("program-name", await platform.ExePrgNameAsync());
            
            await SetNativeAndPeel("pieces-produced", await platform.RdParamDoubleWordNoAxisAsync(6711));
            
            await SetNativeAndPeel("pieces-produced-life", await platform.RdParamDoubleWordNoAxisAsync(6712));
            
            await SetNativeAndPeel("pieces-remaining", await platform.RdParamDoubleWordNoAxisAsync(6713));
            
            await SetNativeAndPeel("cycle-time", await platform.RdParamDoubleWordNoAxisAsync(6757));
            
            await SetNativeAndPeel("cycle-time-ms", await platform.RdParamDoubleWordNoAxisAsync(6758));
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