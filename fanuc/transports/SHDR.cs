using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;
using MTConnect.Adapters.Shdr;
using MTConnect.Streams;
using NLog.Targets;
using Scriban;
using Scriban.Runtime;

namespace l99.driver.fanuc.transports
{
    public class SHDR : Transport
    {
        private dynamic _config;

        private Dictionary<string, (List<string>, List<string>)> _machineStructure =
            new Dictionary<string, (List<string>, List<string>)>();
        
        // config - veneer type, template text
        private Dictionary<string, string> _transformLookup = new Dictionary<string, string>();
        
        // runtime - veneer name, template
        private Dictionary<string, Template> _templateLookup = new Dictionary<string, Template>();
        
        private ShdrAdapter _adapter;
        
        private ScriptObject _globalScriptObject;
        private TemplateContext _globalTemplateContext;

        public SHDR(Machine machine, object cfg) : base(machine, cfg)
        {
            _config = cfg;
        }

        public override async Task<dynamic?> CreateAsync()
        {
            _adapter = new ShdrAdapter(
                _config.transport["device_name"],
                _config.transport["net"]["port"],
                _config.transport["net"]["heartbeat_ms"]);
            
            _adapter.Start();

            _globalScriptObject = new ScriptObject();
            
            _globalScriptObject.Import("ShdrSample", 
                new Action<string,object> ((k,v) =>
                {
                    Console.WriteLine($"{k}:{v}");
                    _adapter.AddDataItem(k,v); 
                }));
            
            _globalScriptObject.Import("ShdrSampleUnavailable", 
                new Action<string> ((k) =>
                {
                    _adapter.AddDataItem(k,"UNAVAILABLE"); 
                }));
            
            _globalScriptObject.Import("ShdrEvent", 
                new Action<string,object> ((k,v) =>
                {
                    Console.WriteLine($"{k}:{v}");
                    _adapter.AddDataItem(k,v); 
                }));
            
            _globalScriptObject.Import("ShdrEventUnavailable", 
                new Action<string> ((k) =>
                {
                    _adapter.AddDataItem(k,"UNAVAILABLE"); 
                }));
            
            _globalScriptObject.Import("ShdrConditionNormal", 
                new Action<string> ((k) =>
                {
                    _adapter.AddCondition(new ShdrCondition(k, ConditionLevel.NORMAL)); 
                }));
            
            _globalScriptObject.Import("ShdrConditionFault", 
                new Action<string> ((k) =>
                {
                    _adapter.AddCondition(new ShdrCondition(k, ConditionLevel.FAULT)); 
                }));
            
            _globalScriptObject.Import("ShdrConditionUnavailable", 
                new Action<string> ((k) =>
                {
                    _adapter.AddCondition(new ShdrCondition(k, ConditionLevel.UNAVAILABLE)); 
                }));
            
            _globalScriptObject.Import("ShdrAllUnavailable", 
                new Action (() =>
                {
                    _adapter.SetUnavailable(); 
                }));

            _globalTemplateContext = new TemplateContext();
            _globalTemplateContext.PushGlobal(_globalScriptObject);
            
            _transformLookup = (_config.transport["transformers"] as Dictionary<dynamic,dynamic>)
                .ToDictionary(
                    kv => (string)kv.Key, 
                    kv => (string)kv.Value);
            
            return null;
        }

        public override async Task StrategyInitializedAsync(dynamic? strategyInit)
        {
            _machineStructure = strategyInit;
        }

        public override async Task ConnectAsync()
        {
            
        }
        
        public override async Task SendAsync(params dynamic[] parameters)
        {
            var @event = parameters[0];
            var veneer = parameters[1];
            var data = parameters[2];

            switch (@event)
            {
                case "DATA_ARRIVE": 
                case "DATA_CHANGE":

                    string transformName = 
                        $"{veneer.GetType().FullName}, {veneer.GetType().Assembly.GetName().Name}";
                    
                    if (_transformLookup.ContainsKey(transformName))
                    {
                        _globalScriptObject.SetValue("observation", data.observation, true);
                        _globalScriptObject.SetValue("data", data.state.data, true);
                        await Template.EvaluateAsync(_transformLookup[transformName], _globalTemplateContext);
                    }
                    
                    break;
                
                case "SWEEP_END":

                    if (_transformLookup.ContainsKey("SWEEP_END"))
                    {
                        _globalScriptObject.SetValue("observation", data.observation, true);
                        _globalScriptObject.SetValue("data", data.state.data, true);
                        await Template.EvaluateAsync(_transformLookup["SWEEP_END"], _globalTemplateContext);
                    }
                    
                    break;
                
                case "INT_MODEL":

                    break;
            }
        }
    }
}