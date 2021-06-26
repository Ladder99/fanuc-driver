using System;
using Newtonsoft.Json.Linq;
using NLog;

namespace l99.driver.fanuc.collectors
{
    public sealed class NLuaRunnerProxy
    {
        //TODO: Lua async support
        private ILogger _logger;
        private NLuaRunner _runner;

        public Platform Platform => _runner.Platform;
        
        public NLuaRunnerProxy(NLuaRunner runner)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _runner = runner;
        }

        public void log(string message, int severity = 7)
        {
            LogLevel ll = LogLevel.Trace;

            if(severity>7) ll = LogLevel.Trace;
            else if(severity==7) ll = LogLevel.Debug;
            else if (severity==6||severity==5) ll = LogLevel.Info;
            else if (severity==4) ll = LogLevel.Warn;
            else if (severity==3||severity==2||severity==1) ll = LogLevel.Error;
            else ll = LogLevel.Fatal;

            _logger.Log(ll, message);
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

        /*public async Task apply(string veneerType, string veneerName, bool isCompound = false, bool isInternal = false)
        {
            await _runner.Apply(veneerType, veneerName, isCompound, isInternal);
        }*/
        
        public void apply(string veneerType, string veneerName, bool isCompound = false, bool isInternal = false)
        {
            _runner.Apply(veneerType, veneerName, isCompound, isInternal);
        }

        public dynamic? get(string propertyBagKey)
        {
            return _runner.Get(propertyBagKey);
        }
        
        /*public async Task<dynamic?> set(string propertyBagKey, dynamic? value)
        {
            return await _runner.Set(propertyBagKey, value);
        }*/
        
        public dynamic? set(string propertyBagKey, dynamic? value)
        {
            return _runner.Set(propertyBagKey, value).GetAwaiter().GetResult();
        }
        
        /*public async Task<dynamic?> set_native(string propertyBagKey, dynamic? value)
        {
            return await _runner.SetNative(propertyBagKey, value);
        }*/
        
        public dynamic? set_native(string propertyBagKey, dynamic? value)
        {
            return _runner.SetNative(propertyBagKey, value).GetAwaiter().GetResult();
        }
        
        /*public async Task<dynamic?> set_native_and_peel(string propertyBagKey, dynamic? value)
        {
            return await _runner.SetNativeAndPeel(propertyBagKey, value);
        }*/
        
        public dynamic? set_native_and_peel(string propertyBagKey, dynamic? value)
        {
            return _runner.SetNativeAndPeel(propertyBagKey, value).GetAwaiter().GetResult();
        }
        
        /*public async Task<dynamic?> peel(string veneerKey, params dynamic[] inputs)
        {
            return await _runner.Peel(veneerKey, inputs);
        }*/
        
        public dynamic? peel(string veneerKey, params dynamic[] inputs)
        {
            return _runner.Peel(veneerKey, inputs).GetAwaiter().GetResult();
        }
    }
}