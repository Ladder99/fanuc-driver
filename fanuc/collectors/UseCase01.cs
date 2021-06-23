using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.collectors
{
    public class UseCase01 : FanucCollector2
    {
        public UseCase01(Machine machine, int sweepMs = 1000, params dynamic[] additionalParams) : base(machine, sweepMs, additionalParams)
        {
            
        }
        
        public override async Task InitRootAsync()
        {
            Apply(typeof(fanuc.veneers.Alarms), "alarms");
            
            Apply(typeof(fanuc.veneers.Alarms2), "alarms2");
            
            Apply(typeof(fanuc.veneers.OpMsgs), "message1");
            
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
            await SetNativeAndPeel("alarms", await platform.RdAlmMsgAllAsync(10,20));
            await SetNativeAndPeel("alarms2", await platform.RdAlmMsg2AllAsync(10,20));
                    
            await SetNativeAndPeel("message1", await platform.RdOpMsgAsync(0, 6+256));
            //await SetNativeAndPeel("message2", await platform.RdOpMsgAsync(1, 6+256));
            
            //var a = await platform.RdOpMsg1_15_15i_Async();
            //var b = await platform.RdOpMsg2_15_15i_Async();
            //var c = await platform.RdOpMsg3_15_15i_Async();
            //var d = await platform.RdOpMsg4_15_15i_Async();
            //var e = await platform.RdOpMsgMacro_15_15i_Async();
            //var f = await platform.RdOpMsgAll_15_15i_Async();
            //var g = await platform.RdOpMsg1_16i_18iW_Async();
            //var h = await platform.RdOpMsg2_16i_18iW_Async();
            //var i = await platform.RdOpMsg3_16i_18iW_Async();
            //var j = await platform.RdOpMsg4_16i_18iW_Async();
            //var k = await platform.RdOpMsgAll_16i_18iW_Async();
            //var l = await platform.RdOpMsg1_16_18_21_16i_18i_21i_0i_30i_PowerMatei_PMiA_Async();

        }

        public override async Task CollectForEachPathAsync(short current_path, dynamic path_marker)
        {
            
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