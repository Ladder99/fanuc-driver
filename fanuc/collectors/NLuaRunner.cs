using System;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json.Linq;
using NLua;
using NLua.Exceptions;

namespace l99.driver.fanuc.collectors
{
    public class NLuaRunner : FanucCollector2
    {
        
        public bool IsValid
        {
            get => _lua_script_available && _lua_string_valid;
        }

        private bool _lua_string_valid = false;
        private bool _lua_script_available = false;
        private string _lua_script_path = string.Empty;
        private Lua _lua_state;
        private LuaTable _lua_table_collector;
        private LuaFunction _lua_fnc_init_root;
        private LuaFunction _lua_fnc_init_path;
        private LuaFunction _lua_fnc_init_axis_and_spindle;
        private LuaFunction _lua_fnc_collect_root;
        private LuaFunction _lua_fnc_collect_path;
        private LuaFunction _lua_fnc_collect_axis;
        private LuaFunction _lua_fnc_collect_spindle;
        
        public Platform Platform
        {
            get => this._platform;
        }
        
        public NLuaRunner(Machine machine, int sweepMs = 1000, params dynamic[] additional_params) : base(machine, sweepMs, additional_params)
        {
            _lua_script_path = additional_params[0];
            Console.WriteLine(_lua_script_path);
            var ok = _create_lua_state_from_file(_lua_script_path);
        }

        private bool _create_lua_state_from_file(string file_path)
        {
            if (!System.IO.File.Exists(file_path))
            {
                _lua_script_available = false;
                return _lua_script_available;
            }
            else
            {
                _lua_script_available = true;
                _lua_state = new Lua();
                _lua_state.LoadCLRPackage();
                return _lua_script_available && _load_lua_state(System.IO.File.ReadAllText(file_path));
            }
        }
        
        private bool _create_lua_state_from_string(string script_string)
        {
            _lua_script_available = true;
            _lua_state = new Lua();
            _lua_state.LoadCLRPackage();
            return _lua_script_available && _load_lua_state(script_string);
        }
        
        private bool _load_lua_state(string script_text)
        {
            try
            {
                _lua_state.DoString(script_text);
                _lua_string_valid = true;
                
                _lua_table_collector = _lua_state["script"] as LuaTable;
                _lua_fnc_init_root = _lua_table_collector?["init_root"] as LuaFunction;
                _lua_fnc_init_path = _lua_table_collector?["init_paths"] as LuaFunction;
                _lua_fnc_init_axis_and_spindle = _lua_table_collector?["init_axis_and_spindle"] as LuaFunction;
                _lua_fnc_collect_root = _lua_table_collector?["collect_root"] as LuaFunction;
                _lua_fnc_collect_path = _lua_table_collector?["collect_path"] as LuaFunction;
                _lua_fnc_collect_axis = _lua_table_collector?["collect_axis"] as LuaFunction;
                _lua_fnc_collect_spindle = _lua_table_collector?["collect_spindle"] as LuaFunction;
                
                //TODO: warn about script syntax
            }
            catch (LuaScriptException lse)
            {
                _lua_string_valid = false;
            }

            return _lua_string_valid;
        }

        public bool publish(string topic, dynamic payload, bool retained = false)
        {
            try
            {
                var payload_string = payload.GetType().Namespace == null
                    ? JObject.FromObject(payload).ToString()
                    : payload.ToString();
                _machine.Broker.PublishAsync(topic, payload_string, retained);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public override async Task InitRootAsync()
        {
            try
            {
                var r = _lua_fnc_init_root?.Call(null, _lua_table_collector, this);
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
                var r = _lua_fnc_init_path?.Call(null, _lua_table_collector, this);
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
                var r = _lua_fnc_init_axis_and_spindle?.Call(null, _lua_table_collector, this);
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
                var r = _lua_fnc_collect_root?.Call(null, _lua_table_collector, this);
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
                var r = _lua_fnc_collect_path?.Call(null, _lua_table_collector, this, current_path);
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
                var r = _lua_fnc_collect_axis?.Call(null, _lua_table_collector, this, get("current_path"), current_axis, axis_name);
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
                var r = _lua_fnc_collect_spindle?.Call(null, _lua_table_collector, this, get("current_path"), current_spindle, spindle_name);
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