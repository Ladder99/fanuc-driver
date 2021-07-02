using System;
using NLua;
using NLua.Exceptions;
using NLog;

namespace l99.driver.fanuc.collectors
{
    public sealed class NLuaRunnerSystemScript
    {
        private ILogger _logger;
        
        private Lua _luaState;
        public LuaTable Table { get; private set; }
        public LuaFunction FncInitRoot { get; private set; }
        public LuaFunction FncInitPath { get; private set; }
        public LuaFunction FncInitAxisAndSpindle { get; private set; }
        public LuaFunction FncPostInit { get; private set; }
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
            _logger = LogManager.GetCurrentClassLogger();
            _luaState = lua;
            ScriptPath = scriptPath;
            _logger.Info($"System script path '{scriptPath}'");
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
                _logger.Trace(scriptText);
                _luaState.DoString(scriptText);
                Table = _luaState["script"] as LuaTable;
                FncInitRoot = Table?["init_root"] as LuaFunction;
                FncInitPath = Table?["init_paths"] as LuaFunction;
                FncInitAxisAndSpindle = Table?["init_axis_and_spindle"] as LuaFunction;
                FncPostInit = Table?["init_post"] as LuaFunction;
                FncCollectRoot = Table?["collect_root"] as LuaFunction;
                FncCollectPath = Table?["collect_path"] as LuaFunction;
                FncCollectAxis = Table?["collect_axis"] as LuaFunction;
                FncCollectSpindle = Table?["collect_spindle"] as LuaFunction;
                
                //TODO: warn about script syntax
            }
            catch (LuaScriptException lse)
            {
                _logger.Warn(lse, "Injecting module failed.");
                return _injectSuccess = false;
            }

            return _injectSuccess = true;
        }
        
    }
}