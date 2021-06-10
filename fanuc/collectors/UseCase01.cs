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
            
            apply(typeof(fanuc.veneers.Alarms2), "alarms2");
            
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
            await set_native_and_peel("alarms2", await _platform.RdAlmMsg2AllAsync(10,20));
                    
            await set_native_and_peel("message1", await _platform.RdOpMsgAsync(0, 6+256));
            //await set_native_and_peel("message2", await _platform.RdOpMsgAsync(1, 6+256));
            
            //var a = await _platform.RdOpMsg1_15_15i_Async();
            //var b = await _platform.RdOpMsg2_15_15i_Async();
            //var c = await _platform.RdOpMsg3_15_15i_Async();
            //var d = await _platform.RdOpMsg4_15_15i_Async();
            //var e = await _platform.RdOpMsgMacro_15_15i_Async();
            //var f = await _platform.RdOpMsgAll_15_15i_Async();
            //var g = await _platform.RdOpMsg1_16i_18iW_Async();
            //var h = await _platform.RdOpMsg2_16i_18iW_Async();
            //var i = await _platform.RdOpMsg3_16i_18iW_Async();
            //var j = await _platform.RdOpMsg4_16i_18iW_Async();
            //var k = await _platform.RdOpMsgAll_16i_18iW_Async();
            //var l = await _platform.RdOpMsg1_16_18_21_16i_18i_21i_0i_30i_PowerMatei_PMiA_Async();

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