using System;
using NLua;
using NLua.Exceptions;

namespace l99.driver.fanuc.collectors
{
    public sealed class NLuaRunnerSystemScript
    {
        private Lua _luaState;
        public LuaTable Table { get; private set; }
        public LuaFunction FncInitRoot { get; private set; }
        public LuaFunction FncInitPath { get; private set; }
        public LuaFunction FncInitAxisAndSpindle { get; private set; }
        public LuaFunction FncCollectRoot { get; private set; }
        public LuaFunction FncCollectPath { get; private set; }
        public LuaFunction FncCollectAxis { get; private set; }
        public LuaFunction FncCollectSpindle { get; private set; }

        public bool IsValid
        {
            get => _scriptAvailable && _injectSuccess;
        }

        private bool _injectSuccess = false;
        private bool _scriptAvailable = false;

        public string ScriptPath { get; private set; } = string.Empty;

        public NLuaRunnerSystemScript(Lua lua, string scriptPath)
        {
            _luaState = lua;
            ScriptPath = scriptPath;
            Console.WriteLine(scriptPath);
            injectModule(getScript());
        }
        
        private string getScript()
        {
            if (!System.IO.File.Exists(ScriptPath))
            {
                _scriptAvailable = false;
                return string.Empty;
            }
            else
            {
                _scriptAvailable = true;
                return System.IO.File.ReadAllText(ScriptPath);
            }
        }
        
        private bool injectModule(string scriptText)
        {
            try
            {
                Console.WriteLine(scriptText);
                _luaState.DoString(scriptText);
                Table = _luaState["script"] as LuaTable;
                FncInitRoot = Table?["init_root"] as LuaFunction;
                FncInitPath = Table?["init_paths"] as LuaFunction;
                FncInitAxisAndSpindle = Table?["init_axis_and_spindle"] as LuaFunction;
                FncCollectRoot = Table?["collect_root"] as LuaFunction;
                FncCollectPath = Table?["collect_path"] as LuaFunction;
                FncCollectAxis = Table?["collect_axis"] as LuaFunction;
                FncCollectSpindle = Table?["collect_spindle"] as LuaFunction;
                
                //TODO: warn about script syntax
            }
            catch (LuaScriptException lse)
            {
                return _injectSuccess = false;
            }

            return _injectSuccess = true;
        }
        
    }
}