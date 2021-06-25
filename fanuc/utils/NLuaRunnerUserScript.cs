using System;
using System.Text;
using NLua;
using NLua.Exceptions;

namespace l99.driver.fanuc.collectors
{
    public sealed class NLuaRunnerUserScript
    {
        private Lua _luaState;

        public LuaTable Table { get; private set; }
        public LuaFunction FncCollectRoot { get; private set; }
        public LuaFunction FncCollectPath { get; private set; }
        public LuaFunction FncCollectAxis { get; private set; }
        public LuaFunction FncCollectSpindle { get; private set; }

        private string _luaModuleTemplate =
            @"luanet.load_assembly 'System'

user =  {}";
        
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
            _luaState = lua;
            injectModule(getScript());
        }

        private string getScript()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_luaModuleTemplate);
            sb.AppendLine();
            sb.AppendFormat(_luaFncCollectRootTemplate, "print('collect user root');");
            sb.AppendLine();
            sb.AppendFormat(_luaFncCollectPathTemplate, "print('collect user path ' .. current_path);");
            sb.AppendLine();
            sb.AppendFormat(_luaFncCollectAxisTemplate, "print('collect user axis ' .. current_path .. ' ' .. axis_name);");
            sb.AppendLine();
            sb.AppendFormat(_luaFncCollectSpindleTemplate, "print('collect user spindle ' .. current_path .. ' ' .. spindle_name);");
            sb.AppendLine();
            return sb.ToString();
        }
        
        private bool injectModule(string scriptText)
        {
            try
            {
                Console.WriteLine(scriptText);
                _luaState.DoString(scriptText);
                Table = _luaState["user"] as LuaTable;
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
        
        public bool ModifyUserRootFunction(string user_code)
        {
            string fnc = string.Format(_luaFncCollectRootTemplate, user_code);
            _luaState.DoString(fnc);
            FncCollectRoot = Table?["collect_root"] as LuaFunction;
            
            return true;
        }
        
        public bool ModifyUserPathFunction(string user_code)
        {
            string fnc = string.Format(_luaFncCollectPathTemplate, user_code);
            _luaState.DoString(fnc);
            FncCollectPath = Table?["collect_path"] as LuaFunction;
            
            return true;
        }
        
        public bool ModifyUserAxisFunction(string user_code)
        {
            string fnc = string.Format(_luaFncCollectAxisTemplate, user_code);
            _luaState.DoString(fnc);
            FncCollectAxis = Table?["collect_axis"] as LuaFunction;
            
            return true;
        }
        
        public bool ModifyUserSpindleFunction(string user_code)
        {
            string fnc = string.Format(_luaFncCollectSpindleTemplate, user_code);
            _luaState.DoString(fnc);
            FncCollectSpindle = Table?["collect_spindle"] as LuaFunction;
            
            return true;
        }
    }
}