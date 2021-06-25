using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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
}