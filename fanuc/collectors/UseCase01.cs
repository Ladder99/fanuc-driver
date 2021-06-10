using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.collectors
{
    public class UseCase01 : FanucCollector2
    {
        public UseCase01(Machine machine, int sweepMs = 1000) : base(machine, sweepMs)
        {
            
        }
        
        public override async Task InitRootAsync()
        {
            apply(typeof(fanuc.veneers.Alarms), "alarms");
            
            apply(typeof(fanuc.veneers.OpMsgs), "message1");
            
            //apply(typeof(fanuc.veneers.OpMsgs), "message2");
        }
        
        public override async Task InitPathsAsync()
        {
            
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
            await set_native_and_peel("alarms", await _platform.RdAlmMsgAllAsync(10,20));
                    
            await set_native_and_peel("message1", await _platform.RdOpMsgAsync(0));
            //await set_native_and_peel("message2", await _platform.RdOpMsgAsync(1));
        }

        public override async Task CollectForEachPathAsync(short current_path, dynamic path_marker)
        {
            
        }

        public override async Task CollectForEachAxisAsync(short current_axis, dynamic axis_split, dynamic axis_marker)
        {
            
        }

        public override async Task CollectForEachSpindleAsync(short current_spindle, dynamic spindle_split, dynamic spindle_marker)
        {
            
        }

        public override async Task CollectEndAsync()
        {
            await base.CollectEndAsync();
        }
    }
}