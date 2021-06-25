using System;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;
using MoreLinq;
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
                logger.Debug("Creating MQTT subscriptions.");
                await this.machine.Broker.SubscribeAsync($"fanuc/{machine.Id}/lua/#", onIncomingMessage);
            }
            return result;
        }

        private string _queueFncUserApplyRoot = string.Empty;
        private string _queueFncUserApplyPath = string.Empty;
        private string _queueFncUserApplyAxisAndSpindle = string.Empty;
        private string _queueFncUserCollectRoot = string.Empty;
        private string _queueFncUserCollectPath = string.Empty;
        private string _queueFncUserCollectAxis = string.Empty;
        private string _queueFncUserCollectSpindle = string.Empty;
        
        private async Task onIncomingMessage(string topic, string payload, ushort qos, bool retain)
        {
            logger.Debug($"Received message on topic '{topic}'.");
            logger.Debug(payload);

            //TODO: blah
            var topic_end = string.Join('/',Enumerable.ToArray(Enumerable.TakeLast(topic.Split('/'), 2))).ToLower();
            
            switch (topic.ToLower())
            {
                case "apply/root":
                    _queueFncUserApplyRoot = payload;
                    break;
                
                case "apply/path":
                    _queueFncUserApplyPath = payload;
                    break;
                
                case "apply/axis_spindle":
                    _queueFncUserApplyAxisAndSpindle = payload;
                    break;
                
                case "collect/root":
                    _queueFncUserCollectRoot = payload;
                    break;
                
                case "collect/path":
                    _queueFncUserCollectPath = payload;
                    break;
                
                case "collect/axis":
                    _queueFncUserCollectAxis = payload;
                    break;
                
                case "collect/spindle":
                    _queueFncUserCollectSpindle = payload;
                    break;
            }
        }
        
        public override async Task InitRootAsync()
        {
            try
            {
                var r = _luaSystemScript.FncInitRoot?.Call(null, _luaSystemScript.Table, _luaCollectorProxy);
            }
            catch (Exception e)
            {
                logger.Warn(e, "InitRootAsync Lua invocation failed.");
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
                logger.Warn(e, "InitPathAsync Lua invocation failed.");
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
                logger.Warn(e, "InitAxisAndSpindleAsync Lua invocation failed.");
            }
        }

        public override async Task<bool> CollectBeginAsync()
        {
            var ret = await base.CollectBeginAsync();

            if(!string.IsNullOrEmpty(_queueFncUserApplyRoot))
                _luaUserScript.ModifyInitRootFunction(_queueFncUserApplyRoot);
            
            if(!string.IsNullOrEmpty(_queueFncUserApplyPath))
                _luaUserScript.ModifyInitPathFunction(_queueFncUserApplyPath);
            
            if(!string.IsNullOrEmpty(_queueFncUserApplyAxisAndSpindle))
                _luaUserScript.ModifyInitAxisAndSpindleFunction(_queueFncUserApplyAxisAndSpindle);
            
            if(!string.IsNullOrEmpty(_queueFncUserCollectRoot))
                _luaUserScript.ModifyCollectRootFunction(_queueFncUserCollectRoot);
            
            if(!string.IsNullOrEmpty(_queueFncUserCollectPath))
                _luaUserScript.ModifyCollectPathFunction(_queueFncUserCollectPath);
            
            if(!string.IsNullOrEmpty(_queueFncUserCollectAxis))
                _luaUserScript.ModifyCollectAxisFunction(_queueFncUserCollectAxis);
            
            if(!string.IsNullOrEmpty(_queueFncUserCollectSpindle))
                _luaUserScript.ModifyCollectSpindleFunction(_queueFncUserCollectSpindle);
            
            return ret;
        }

        public override async Task InitUserRootAsync()
        {
            if (!string.IsNullOrEmpty(_queueFncUserApplyRoot))
            {
                logger.Debug("Invoking InitUserRootAsync.");
                
                try
                {
                    var r = _luaUserScript.FncInitRoot?.Call(null, _luaUserScript.Table, _luaCollectorProxy);
                }
                catch (Exception e)
                {
                    logger.Warn(e, "InitUserRootAsync USER Lua invocation failed.");
                }
            }
        }
        
        public override async Task CollectRootAsync()
        {
            try
            {
                var r = _luaSystemScript.FncCollectRoot?.Call(null, _luaSystemScript.Table, _luaCollectorProxy);
            }
            catch (Exception e)
            {
                logger.Warn(e, "CollectRootAsync SYSTEM Lua invocation failed.");
            }
            
            try
            {
                var r = _luaUserScript.FncCollectRoot?.Call(null, _luaUserScript.Table, _luaCollectorProxy);
            }
            catch (Exception e)
            {
                logger.Warn(e, "CollectRootAsync USER Lua invocation failed.");
            }
        }

        public override async Task InitUserPathsAsync()
        {
            if (!string.IsNullOrEmpty(_queueFncUserApplyPath))
            {
                logger.Debug("Invoking InitUserPathsAsync.");
                
                try
                {
                    var r = _luaUserScript.FncInitPath?.Call(null, _luaUserScript.Table, _luaCollectorProxy);
                }
                catch (Exception e)
                {
                    logger.Warn(e, "InitUserPathsAsync USER Lua invocation failed.");
                }
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
                logger.Warn(e, "CollectForEachPathAsync SYSTEM Lua invocation failed.");
            }
            
            try
            {
                var r = _luaUserScript.FncCollectPath?.Call(null, _luaUserScript.Table, _luaCollectorProxy, current_path);
            }
            catch (Exception e)
            {
                logger.Warn(e, "CollectForEachPathAsync USER Lua invocation failed.");
            }
        }

        public override async Task InitUserAxisAndSpindleAsync(short current_path)
        {
            if (!string.IsNullOrEmpty(_queueFncUserApplyAxisAndSpindle))
            {
                logger.Debug("Invoking InitUserAxisAndSpindleAsync.");
                
                try
                {
                    var r = _luaUserScript.FncInitAxisAndSpindle?.Call(null, _luaUserScript.Table, _luaCollectorProxy, current_path);
                }
                catch (Exception e)
                {
                    logger.Warn(e, "InitUserAxisAndSpindleAsync USER Lua invocation failed.");
                }
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
                logger.Warn(e, "CollectForEachAxisAsync SYSTEM Lua invocation failed.");
            }
            
            try
            {
                var r = _luaUserScript.FncCollectAxis?.Call(null, _luaUserScript.Table, _luaCollectorProxy, Get("current_path"), current_axis, axis_name);
            }
            catch (Exception e)
            {
                logger.Warn(e, "CollectForEachAxisAsync USER Lua invocation failed.");
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
                logger.Warn(e, "CollectForEachSpindleAsync SYSTEM Lua invocation failed.");
            }
            
            try
            {
                var r = _luaUserScript.FncCollectSpindle?.Call(null, _luaUserScript.Table, _luaCollectorProxy, Get("current_path"), current_spindle, spindle_name);
            }
            catch (Exception e)
            {
                logger.Warn(e, "CollectForEachSpindleAsync USER Lua invocation failed.");
            }
        }

        public override async Task CollectEndAsync()
        {
            _queueFncUserApplyRoot = string.Empty;
            _queueFncUserApplyPath = string.Empty;
            _queueFncUserApplyAxisAndSpindle = string.Empty;
            _queueFncUserCollectRoot = string.Empty;
            _queueFncUserCollectPath = string.Empty;
            _queueFncUserCollectAxis = string.Empty;
            _queueFncUserCollectSpindle = string.Empty;
            
            await base.CollectEndAsync();
        }
    }
}