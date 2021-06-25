using System;
using System.Text;
using MoreLinq.Extensions;
using NLua;
using NLua.Exceptions;
using NLog;

namespace l99.driver.fanuc.collectors
{
    public sealed class NLuaRunnerUserScript
    {
        private readonly bool EXTRA = false;
        
        private ILogger _logger;
        
        private Lua _luaState;
        public LuaTable Table { get; private set; }
        public LuaFunction FncInitRoot { get; private set; }
        public LuaFunction FncInitPath { get; private set; }
        public LuaFunction FncInitAxisAndSpindle { get; private set; }
        public LuaFunction FncCollectRoot { get; private set; }
        public LuaFunction FncCollectPath { get; private set; }
        public LuaFunction FncCollectAxis { get; private set; }
        public LuaFunction FncCollectSpindle { get; private set; }

        private string _luaModuleTemplate =
            @"luanet.load_assembly 'System'

user =  {}";
        
        private string _luaFncInitRootTemplate =
            @"function user:init_root(this, collector)
    {0} 
end";
        
        private string _luaFncInitPathTemplate =
            @"function user:init_path(this, collector)
    {0} 
end";
        
        private string _luaFncInitAxisAndSpindleTemplate =
            @"function user:init_axis_spindle(this, collector, current_path)
    {0} 
end";
        
        private string _luaFncCollectRootTemplate =
            @"function user:collect_root(this, collector) 
    {0} 
end";

        private string _luaFncCollectPathTemplate =
            @"function user:collect_path(this, collector, current_path)
    {0} 
end";
        
        private string _luaFncCollectAxisTemplate =
            @"function user:collect_axis(this, collector, current_path, current_axis, axis_name)
    {0}
end";
        
        private string _luaFncCollectSpindleTemplate =
            @"function user:collect_spindle(this, collector, current_path, current_spindle, spindle_name)
    {0} 
end";

        public bool IsValid => _injectSuccess;
        
        private bool _injectSuccess = false;
        
        public NLuaRunnerUserScript(Lua lua)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _luaState = lua;
            injectModule(getScript());
        }

        private string getScript()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_luaModuleTemplate);
            sb.AppendLine();
            sb.AppendFormat(_luaFncInitRootTemplate, EXTRA ? "print('init user root');" : "");
            sb.AppendLine();
            sb.AppendFormat(_luaFncInitPathTemplate, EXTRA ? "print('init user path');" : "");
            sb.AppendLine();
            sb.AppendFormat(_luaFncInitAxisAndSpindleTemplate, EXTRA ? "print('init user axis/spindle on path '.. current_path);" : "");
            sb.AppendLine();
            sb.AppendFormat(_luaFncCollectRootTemplate, EXTRA ? "print('collect user root');" : "");
            sb.AppendLine();
            sb.AppendFormat(_luaFncCollectPathTemplate, EXTRA ? "print('collect user path ' .. current_path);" : "");
            sb.AppendLine();
            sb.AppendFormat(_luaFncCollectAxisTemplate, EXTRA ? "print('collect user axis ' .. current_path .. ' ' .. axis_name);" : "");
            sb.AppendLine();
            sb.AppendFormat(_luaFncCollectSpindleTemplate, EXTRA ? "print('collect user spindle ' .. current_path .. ' ' .. spindle_name);" : "");
            sb.AppendLine();
            return sb.ToString();
        }
        
        private bool injectModule(string scriptText)
        {
            try
            {
                _logger.Trace(scriptText);
                _luaState.DoString(scriptText);
                Table = _luaState["user"] as LuaTable;
                FncInitRoot = Table?["init_root"] as LuaFunction;
                FncInitPath = Table?["init_path"] as LuaFunction;
                FncInitAxisAndSpindle = Table?["init_axis_spindle"] as LuaFunction;
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
        
        public bool ModifyInitRootFunction(string user_code)
        {
            try
            {
                string fnc = string.Format(_luaFncInitRootTemplate, user_code);
                _luaState.DoString(fnc);
                FncInitRoot = Table?["init_root"] as LuaFunction;
            
                return true;
            }
            catch (LuaScriptException lse)
            {
                _logger.Warn(lse, "Init root function modification failed.");
                return false;
            }
        }
        
        public bool ModifyInitPathFunction(string user_code)
        {
            try
            {
                string fnc = string.Format(_luaFncInitPathTemplate, user_code);
                _luaState.DoString(fnc);
                FncInitPath = Table?["init_path"] as LuaFunction;
            
                return true;
            }
            catch (LuaScriptException lse)
            {
                _logger.Warn(lse, "Init path function modification failed.");
                return false;
            }
        }
        
        public bool ModifyInitAxisAndSpindleFunction(string user_code)
        {
            try
            {
                string fnc = string.Format(_luaFncInitAxisAndSpindleTemplate, user_code);
                _luaState.DoString(fnc);
                FncInitAxisAndSpindle = Table?["init_axis_spindle"] as LuaFunction;
            
                return true;
            }
            catch (LuaScriptException lse)
            {
                _logger.Warn(lse, "Init axis and spindle function modification failed.");
                return false;
            }
        }
        
        public bool ModifyCollectRootFunction(string user_code)
        {
            try
            {
                string fnc = string.Format(_luaFncCollectRootTemplate, user_code);
                _luaState.DoString(fnc);
                FncCollectRoot = Table?["collect_root"] as LuaFunction;
            
                return true;
            }
            catch (LuaScriptException lse)
            {
                _logger.Warn(lse, "Collect root function modification failed.");
                return false;
            }
        }
        
        public bool ModifyCollectPathFunction(string user_code)
        {
            try
            {
                string fnc = string.Format(_luaFncCollectPathTemplate, user_code);
                _luaState.DoString(fnc);
                FncCollectPath = Table?["collect_path"] as LuaFunction;
            
                return true;
            }
            catch (LuaScriptException lse)
            {
                _logger.Warn(lse, "Collect path function modification failed.");
                return false;
            }
        }
        
        public bool ModifyCollectAxisFunction(string user_code)
        {
            try
            {
                string fnc = string.Format(_luaFncCollectAxisTemplate, user_code);
                _luaState.DoString(fnc);
                FncCollectAxis = Table?["collect_axis"] as LuaFunction;
                
                return true;
            }
            catch (LuaScriptException lse)
            {
                _logger.Warn(lse, "Collect axis function modification failed.");
                return false;
            }
        }
        
        public bool ModifyCollectSpindleFunction(string user_code)
        {
            try
            {
                string fnc = string.Format(_luaFncCollectSpindleTemplate, user_code);
                _luaState.DoString(fnc);
                FncCollectSpindle = Table?["collect_spindle"] as LuaFunction;
                
                return true;
            }
            catch (LuaScriptException lse)
            {
                _logger.Warn(lse, "Collect spindle function modification failed.");
                return false;
            }
        }
    }
}