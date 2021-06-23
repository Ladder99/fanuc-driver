using System;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json.Linq;
using NLua;
using NLua.Exceptions;

namespace l99.driver.fanuc.collectors
{
    public sealed class NLuaRunnerProxy
    {
        private NLuaRunner _runner;

        public Platform Platform => _runner.Platform;
        
        public NLuaRunnerProxy(NLuaRunner runner)
        {
            _runner = runner;
        }
        
        public bool publish(string topic, dynamic payload, bool retained = false)
        {
            try
            {
                var payload_string = payload.GetType().Namespace == null
                    ? JObject.FromObject(payload).ToString()
                    : payload.ToString();
                _runner.Machine.Broker.PublishAsync(topic, payload_string, retained);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public string to_string(dynamic o, string sep = "")
        {
            return String.Join(sep,o);
        }
        
        public string to_json(dynamic o)
        {
            if(o.GetType().IsArray)
                return JArray.FromObject(o).ToString();
            else
                return JObject.FromObject(o).ToString();
        }

        public async Task apply(string veneerType, string veneerName, bool isCompound = false, bool isInternal = false)
        {
            await _runner.Apply(veneerType, veneerName, isCompound, isInternal);
        }

        public dynamic? get(string propertyBagKey)
        {
            return _runner.Get(propertyBagKey);
        }
        
        public async Task<dynamic?> set(string propertyBagKey, dynamic? value)
        {
            return await _runner.Set(propertyBagKey, value);
        }
        
        public async Task<dynamic?> set_native(string propertyBagKey, dynamic? value)
        {
            return await _runner.SetNative(propertyBagKey, value);
        }
        
        public async Task<dynamic?> set_native_and_peel(string propertyBagKey, dynamic? value)
        {
            return await _runner.SetNativeAndPeel(propertyBagKey, value);
        }
        
        public async Task<dynamic?> peel(string veneerKey, params dynamic[] inputs)
        {
            return await _runner.Peel(veneerKey, inputs);
        }
    }

    public class NLuaRunner : FanucCollector2
    {
        private NLuaRunnerProxy _luaCollectorProxy;
        private bool _luaStringValid = false;
        private bool _luaScriptAvailable = false;
        private string _luaScriptPath = string.Empty;
        private Lua _luaState;
        private LuaTable _luaTableCollector;
        private LuaFunction _luaFncInitRoot;
        private LuaFunction _luaFncInitPath;
        private LuaFunction _luaFncInitAxisAndSpindle;
        private LuaFunction _luaFncCollectRoot;
        private LuaFunction _luaFncCollectPath;
        private LuaFunction _luaFncCollectAxis;
        private LuaFunction _luaFncCollectSpindle;
        
        public bool IsValid
        {
            get => _luaScriptAvailable && _luaStringValid;
        }
        
        public Platform Platform
        {
            get => this.platform;
        }
        
        public NLuaRunner(Machine machine, int sweepMs = 1000, params dynamic[] additionalParams) : base(machine, sweepMs, additionalParams)
        {
            _luaCollectorProxy = new NLuaRunnerProxy(this);
            _luaScriptPath = additionalParams[0];
            Console.WriteLine(_luaScriptPath);
            var ok = createLuaStateFromFile(_luaScriptPath);
        }

        private bool createLuaStateFromFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                _luaScriptAvailable = false;
                return _luaScriptAvailable;
            }
            else
            {
                _luaScriptAvailable = true;
                _luaState = new Lua();
                _luaState.LoadCLRPackage();
                return _luaScriptAvailable && loadLuaState(System.IO.File.ReadAllText(filePath));
            }
        }
        
        private bool createLuaStateFromString(string scriptString)
        {
            _luaScriptAvailable = true;
            _luaState = new Lua();
            _luaState.LoadCLRPackage();
            return _luaScriptAvailable && loadLuaState(scriptString);
        }
        
        private bool loadLuaState(string scriptText)
        {
            try
            {
                _luaState.DoString(scriptText);
                _luaStringValid = true;
                
                _luaTableCollector = _luaState["script"] as LuaTable;
                _luaFncInitRoot = _luaTableCollector?["init_root"] as LuaFunction;
                _luaFncInitPath = _luaTableCollector?["init_paths"] as LuaFunction;
                _luaFncInitAxisAndSpindle = _luaTableCollector?["init_axis_and_spindle"] as LuaFunction;
                _luaFncCollectRoot = _luaTableCollector?["collect_root"] as LuaFunction;
                _luaFncCollectPath = _luaTableCollector?["collect_path"] as LuaFunction;
                _luaFncCollectAxis = _luaTableCollector?["collect_axis"] as LuaFunction;
                _luaFncCollectSpindle = _luaTableCollector?["collect_spindle"] as LuaFunction;
                
                //TODO: warn about script syntax
            }
            catch (LuaScriptException lse)
            {
                _luaStringValid = false;
            }

            return _luaStringValid;
        }

        public override async Task InitRootAsync()
        {
            try
            {
                var r = _luaFncInitRoot?.Call(null, _luaTableCollector, _luaCollectorProxy);
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
                var r = _luaFncInitPath?.Call(null, _luaTableCollector, _luaCollectorProxy);
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
                var r = _luaFncInitAxisAndSpindle?.Call(null, _luaTableCollector, _luaCollectorProxy);
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
                var r = _luaFncCollectRoot?.Call(null, _luaTableCollector, _luaCollectorProxy);
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
                var r = _luaFncCollectPath?.Call(null, _luaTableCollector, _luaCollectorProxy, current_path);
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
                var r = _luaFncCollectAxis?.Call(null, _luaTableCollector, _luaCollectorProxy, Get("current_path"), current_axis, axis_name);
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
                var r = _luaFncCollectSpindle?.Call(null, _luaTableCollector, _luaCollectorProxy, Get("current_path"), current_spindle, spindle_name);
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