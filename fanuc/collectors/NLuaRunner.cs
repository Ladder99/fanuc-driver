using System;
using System.Threading.Tasks;
using l99.driver.@base;
using NLua;

namespace l99.driver.fanuc.collectors
{
    public class NLuaRunner : FanucCollector2
    {
        private NLuaRunnerProxy _luaCollectorProxy;
        private NLuaRunnerSystemScript _luaSystemScript;
        private NLuaRunnerUserScript _luaUserScript;
        
        private Lua _luaState;
        
        public Platform Platform
        {
            get => this.platform;
        }
        
        public NLuaRunner(Machine machine, int sweepMs = 1000, params dynamic[] additionalParams) : base(machine, sweepMs, additionalParams)
        {
            _luaState = new Lua();
            _luaState.LoadCLRPackage();
            
            _luaCollectorProxy = new NLuaRunnerProxy(this);
            _luaSystemScript = new NLuaRunnerSystemScript(_luaState, additionalParams[0]);
            logger.Info($"Lua SYSTEM script valid: {_luaSystemScript.IsValid}");
            _luaUserScript = new NLuaRunnerUserScript(_luaState);
            logger.Info($"Lua USER script valid: {_luaUserScript.IsValid}");
            
        }

        public override async Task<dynamic?> InitializeAsync()
        {
            var result = base.InitializeAsync();
            if (machine.VeneersApplied)
            {
                await this.machine.Broker.SubscribeAsync("fanuc/lua", onIncomingMessage);
            }
            return result;
        }

        private async Task onIncomingMessage(string topic, string payload, ushort qos, bool retain)
        {
            Console.WriteLine("------");
            Console.WriteLine(topic);
            Console.WriteLine(payload);
            Console.WriteLine("------");
        }
        
        public override async Task InitRootAsync()
        {
            try
            {
                var r = _luaSystemScript.FncInitRoot?.Call(null, _luaSystemScript.Table, _luaCollectorProxy);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        public override async Task InitPathsAsync()
        {
            try
            {
                var r = _luaSystemScript.FncInitPath?.Call(null, _luaSystemScript.Table, _luaCollectorProxy);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        public override async Task InitAxisAndSpindleAsync()
        {
            try
            {
                var r = _luaSystemScript.FncInitAxisAndSpindle?.Call(null, _luaSystemScript.Table, _luaCollectorProxy);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override async Task<bool> CollectBeginAsync()
        {
            return await base.CollectBeginAsync();
        }

        public override async Task CollectRootAsync()
        {
            try
            {
                var r = _luaSystemScript.FncCollectRoot?.Call(null, _luaSystemScript.Table, _luaCollectorProxy);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            try
            {
                var r = _luaUserScript.FncCollectRoot?.Call(null, _luaUserScript.Table, _luaCollectorProxy);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override async Task CollectForEachPathAsync(short current_path, dynamic path_marker)
        {
            try
            {
                var r = _luaSystemScript.FncCollectPath?.Call(null, _luaSystemScript.Table, _luaCollectorProxy, current_path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            try
            {
                var r = _luaUserScript.FncCollectPath?.Call(null, _luaUserScript.Table, _luaCollectorProxy, current_path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override async Task CollectForEachAxisAsync(short current_axis, string axis_name, dynamic axis_split, dynamic axis_marker)
        {
            try
            {
                var r = _luaSystemScript.FncCollectAxis?.Call(null, _luaSystemScript.Table, _luaCollectorProxy, Get("current_path"), current_axis, axis_name);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            try
            {
                var r = _luaUserScript.FncCollectAxis?.Call(null, _luaUserScript.Table, _luaCollectorProxy, Get("current_path"), current_axis, axis_name);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override async Task CollectForEachSpindleAsync(short current_spindle, string spindle_name, dynamic spindle_split, dynamic spindle_marker)
        {
            try
            {
                var r = _luaSystemScript.FncCollectSpindle?.Call(null, _luaUserScript.Table, _luaCollectorProxy, Get("current_path"), current_spindle, spindle_name);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            try
            {
                var r = _luaUserScript.FncCollectSpindle?.Call(null, _luaUserScript.Table, _luaCollectorProxy, Get("current_path"), current_spindle, spindle_name);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override async Task CollectEndAsync()
        {
            await base.CollectEndAsync();
        }
    }
}