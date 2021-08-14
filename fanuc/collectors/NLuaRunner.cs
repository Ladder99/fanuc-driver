using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json.Linq;
using NLua;

namespace l99.driver.fanuc.collectors
{
    public class NLuaRunner : FanucCollector2
    {
        private NLuaRunnerProxy _luaCollectorProxy;
        private NLuaRunnerSystemScript _luaSystemScript;
        private NLuaRunnerUserScript _luaUserScript;
        
        private Lua _luaState;

        private Dictionary<string, dynamic> _userScriptDef;
        
        public Platform Platform
        {
            get => this.platform;
        }
        
        public NLuaRunner(Machine machine, object cfg) : base(machine, cfg)
        {
            dynamic config = (dynamic)cfg;
            
            _luaState = new Lua();
            _luaState.LoadCLRPackage();
            
            _luaCollectorProxy = new NLuaRunnerProxy(this);
            
            _luaSystemScript = new NLuaRunnerSystemScript(_luaState, config.strategy["script"]);
            logger.Info($"Lua SYSTEM script valid: {_luaSystemScript.IsValid}");
            
            _luaUserScript = new NLuaRunnerUserScript(_luaState);
            logger.Info($"Lua USER script valid: {_luaUserScript.IsValid}");
            
            _userScriptDef = new Dictionary<string, dynamic>()
            {
                { "apply/root", new
                    {
                        topic = $"fanuc/{machine.Id}/lua/apply/root",
                        lua = new
                        {
                            call = new Func<LuaFunction>(() => { return _luaUserScript.FncInitRoot; }),
                            modify = new Func<string, bool>((script) => { return _luaUserScript.ModifyInitRootFunction(script); }),
                            queued = new Queue<string>(),
                            dequeued = new Queue<string>(),
                            history = new List<dynamic>()
                        }
                    } 
                },
                { "apply/path", new
                    {
                        topic = $"fanuc/{machine.Id}/lua/apply/path",
                        lua = new
                        {
                            call = new Func<LuaFunction>(() => { return _luaUserScript.FncInitPath; }),
                            modify = new Func<string, bool>((script) => { return _luaUserScript.ModifyInitPathFunction(script); }),
                            queued = new Queue<string>(),
                            dequeued = new Queue<string>(),
                            history = new List<dynamic>()
                        }
                    } 
                },
                { "apply/axis_spindle", new
                    {
                        topic = $"fanuc/{machine.Id}/lua/apply/axis_spindle",
                        lua = new
                        {
                            call = new Func<LuaFunction>(() => { return _luaUserScript.FncInitAxisAndSpindle; }),
                            modify = new Func<string, bool>((script) => { return _luaUserScript.ModifyInitAxisAndSpindleFunction(script); }),
                            queued = new Queue<string>(),
                            dequeued = new Queue<string>(),
                            history = new List<dynamic>()
                        }
                    } 
                },
                { "collect/root", new
                    {
                        topic = $"fanuc/{machine.Id}/lua/collect/root",
                        lua = new
                        {
                            call = new Func<LuaFunction>(() => { return _luaUserScript.FncCollectRoot; }),
                            modify = new Func<string, bool>((script) => { return _luaUserScript.ModifyCollectRootFunction(script); }),
                            queued = new Queue<string>(),
                            dequeued = new Queue<string>(),
                            history = new List<dynamic>()
                        }
                    } 
                },
                { "collect/path", new
                    {
                        topic = $"fanuc/{machine.Id}/lua/collect/path",
                        lua = new
                        {
                            call = new Func<LuaFunction>(() => { return _luaUserScript.FncCollectPath; }),
                            modify = new Func<string, bool>((script) => { return _luaUserScript.ModifyCollectPathFunction(script); }),
                            queued = new Queue<string>(),
                            dequeued = new Queue<string>(),
                            history = new List<dynamic>()
                        }
                    } 
                },
                { "collect/axis", new
                    {
                        topic = $"fanuc/{machine.Id}/lua/collect/axis",
                        lua = new
                        {
                            call = new Func<LuaFunction>(() => { return _luaUserScript.FncCollectAxis; }),
                            modify = new Func<string, bool>((script) => { return _luaUserScript.ModifyCollectAxisFunction(script); }),
                            queued = new Queue<string>(),
                            dequeued = new Queue<string>(),
                            history = new List<dynamic>()
                        }
                    } 
                },
                { "collect/spindle", new
                    {
                        topic = $"fanuc/{machine.Id}/lua/collect/spindle",
                        lua = new
                        {
                            call = new Func<LuaFunction>(() => { return _luaUserScript.FncCollectSpindle; }),
                            modify = new Func<string, bool>((script) => { return _luaUserScript.ModifyCollectSpindleFunction(script); }),
                            queued = new Queue<string>(),
                            dequeued = new Queue<string>(),
                            history = new List<dynamic>()
                        }
                    } 
                }
            };
        }

        private string createError(long ts, Exception outer, Exception inner)
        {
            return JObject.FromObject(new
            {
                timestamp = ts,
                error = new
                {
                    outer = new {outer?.Message, outer?.StackTrace},
                    inner = new {inner?.Message, inner?.StackTrace}
                }
            }).ToString();
        }
        
        public override async Task<dynamic?> InitializeAsync()
        {
            var result = base.InitializeAsync();
            if (machine.VeneersApplied)
            {
                logger.Debug("Creating MQTT subscriptions.");
                foreach (var kv in _userScriptDef)
                {
                    await this.machine.Broker.SubscribeAsync((string)kv.Value.topic, onIncomingMessage);
                }
                
            }
            return result;
        }

        private async Task onIncomingMessage(string topic, string payload, ushort qos, bool retain)
        {
            logger.Debug($"'{topic}' - Received message.");
            logger.Debug(payload);

            //TODO: blah
            var topic_end = string.Join('/',Enumerable.ToArray(Enumerable.TakeLast(topic.Split('/'), 2))).ToLower();

            if (_userScriptDef[topic_end].lua.queued.Count == 0)
                _userScriptDef[topic_end].lua.queued.Enqueue(payload);
            else
            {
                var ts = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                await machine.Broker.PublishAsync(
                    $"{_userScriptDef[topic_end].topic}/{ts}",
                    createError(ts, new Exception("Queue pending. Message was rejected."), null));
                logger.Warn($"'{topic}' - Queue pending. Message was rejected.");
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
                if(e.InnerException!=null) logger.Warn(e.InnerException);
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
                if(e.InnerException!=null) logger.Warn(e.InnerException);
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
                if(e.InnerException!=null) logger.Warn(e.InnerException);
            }
        }

        public override async Task PostInitAsync()
        {
            try
            {
                var r = _luaSystemScript.FncPostInit?.Call(null, _luaSystemScript.Table, _luaCollectorProxy);
            }
            catch (Exception e)
            {
                logger.Warn(e, "PostInitAsync SYSTEM Lua invocation failed.");
                if(e.InnerException!=null) logger.Warn(e.InnerException);
            }
        }
        
        public override async Task<bool> CollectBeginAsync()
        {
            var ret = await base.CollectBeginAsync();

            foreach (var kv in _userScriptDef)
            {
                if (kv.Value.lua.queued.Count > 0)
                {
                    string payload = kv.Value.lua.queued.Dequeue();
                    kv.Value.lua.dequeued.Enqueue(payload);
                    var accepted = kv.Value.lua.modify(payload);
                    var history = new
                    {
                        timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                        status = new 
                        {
                            chunk = payload,
                            accepted 
                        }
                    };
                    kv.Value.lua.history.Add(history);
                    await machine.Broker.PublishAsync($"{kv.Value.topic}/{history.timestamp}", JObject.FromObject(history).ToString(), true );
                }
            }
            
            return ret;
        }

        public override async Task InitUserRootAsync()
        {
            var v = _userScriptDef["apply/root"];
            
            if (v.lua.dequeued.Count > 0)
            {
                logger.Debug("Invoking InitUserRootAsync.");
                
                try
                {
                    var r = v.lua.call()?.Call(null, _luaUserScript.Table, _luaCollectorProxy);
                }
                catch (Exception e)
                {
                    logger.Warn(e, "InitUserRootAsync USER Lua invocation failed.");
                    var ts = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    await machine.Broker.PublishAsync(
                        $"{_userScriptDef["apply/root"].topic}/{ts}", 
                        createError(ts, e, e.InnerException), true );
                    if (e.InnerException != null)
                        logger.Warn(e.InnerException);
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
                if(e.InnerException!=null) logger.Warn(e.InnerException);
            }
            
            try
            {
                var r = _userScriptDef["collect/root"].lua.call()?.Call(null, _luaUserScript.Table, _luaCollectorProxy);
            }
            catch (Exception e)
            {
                logger.Warn(e, "CollectRootAsync USER Lua invocation failed.");
                if (_userScriptDef["collect/root"].lua.dequeued.Count > 0)
                {
                    var ts = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    await machine.Broker.PublishAsync(
                        $"{_userScriptDef["collect/root"].topic}/{ts}",
                        createError(ts, e, e.InnerException), true);
                }
                if (e.InnerException != null)
                    logger.Warn(e.InnerException);
            }
        }

        public override async Task InitUserPathsAsync()
        {
            var v = _userScriptDef["apply/path"];
            
            if (v.lua.dequeued.Count > 0)
            {
                logger.Debug("Invoking InitUserPathsAsync.");
                
                try
                {
                    var r = v.lua.call()?.Call(null, _luaUserScript.Table, _luaCollectorProxy);
                }
                catch (Exception e)
                {
                    logger.Warn(e, "InitUserPathsAsync USER Lua invocation failed.");
                    var ts = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    await machine.Broker.PublishAsync(
                        $"{_userScriptDef["apply/path"].topic}/{ts}", 
                        createError(ts, e, e.InnerException), true );
                    if (e.InnerException != null)
                        logger.Warn(e.InnerException);
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
                if(e.InnerException!=null) logger.Warn(e.InnerException);
            }
            
            try
            {
                var r = _userScriptDef["collect/path"].lua.call()?.Call(null, _luaUserScript.Table, _luaCollectorProxy, current_path);
            }
            catch (Exception e)
            {
                logger.Warn(e, "CollectForEachPathAsync USER Lua invocation failed.");
                if (_userScriptDef["collect/path"].lua.dequeued.Count > 0)
                {
                    var ts = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    await machine.Broker.PublishAsync(
                        $"{_userScriptDef["collect/path"].topic}/{ts}",
                        createError(ts, e, e.InnerException), true);
                }
                if (e.InnerException != null)
                    logger.Warn(e.InnerException);
            }
        }

        public override async Task InitUserAxisAndSpindleAsync(short current_path)
        {
            var v = _userScriptDef["apply/axis_spindle"];
            
            if (v.lua.dequeued.Count > 0)
            {
                logger.Debug("Invoking InitUserAxisAndSpindleAsync.");
                
                try
                {
                    var r = v.lua.call()?.Call(null, _luaUserScript.Table, _luaCollectorProxy, current_path);
                }
                catch (Exception e)
                {
                    logger.Warn(e, "InitUserAxisAndSpindleAsync USER Lua invocation failed.");
                    var ts = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    await machine.Broker.PublishAsync(
                        $"{_userScriptDef["apply/axis_spindle"].topic}/{ts}", 
                        createError(ts, e, e.InnerException), true );
                    if (e.InnerException != null)
                        logger.Warn(e.InnerException);
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
                if(e.InnerException!=null) logger.Warn(e.InnerException);
            }
            
            try
            {
                var r = _userScriptDef["collect/axis"].lua.call()?.Call(null, _luaUserScript.Table, _luaCollectorProxy, Get("current_path"), current_axis, axis_name);
            }
            catch (Exception e)
            {
                logger.Warn(e, "CollectForEachAxisAsync USER Lua invocation failed.");
                if (_userScriptDef["collect/axis"].lua.dequeued.Count > 0)
                {
                    var ts = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    await machine.Broker.PublishAsync(
                        $"{_userScriptDef["collect/axis"].topic}/{ts}",
                        createError(ts, e, e.InnerException), true);
                }
                if (e.InnerException != null)
                    logger.Warn(e.InnerException);
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
                if(e.InnerException!=null) logger.Warn(e.InnerException);
            }
            
            try
            {
                var r = _userScriptDef["collect/spindle"].lua.call()?.Call(null, _luaUserScript.Table, _luaCollectorProxy, Get("current_path"), current_spindle, spindle_name);
            }
            catch (Exception e)
            {
                logger.Warn(e, "CollectForEachSpindleAsync USER Lua invocation failed.");
                if (_userScriptDef["collect/spindle"].lua.dequeued.Count > 0)
                {
                    var ts = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    await machine.Broker.PublishAsync(
                        $"{_userScriptDef["collect/spindle"].topic}/{ts}",
                        createError(ts, e, e.InnerException), true);
                }
                if (e.InnerException != null)
                    logger.Warn(e.InnerException);
            }
        }

        public override async Task CollectEndAsync()
        {
            foreach (var kv in _userScriptDef)
            {
                if (kv.Value.lua.dequeued.Count > 0)
                {
                    kv.Value.lua.dequeued.Dequeue();
                }
            }
            
            await base.CollectEndAsync();
        }
    }
}