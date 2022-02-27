using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using l99.driver.@base;
using MTConnect.Adapters.Shdr;
using MTConnect.Streams;
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
        private AdapterInfo _adapterInfo;
        
        private ScriptObject _globalScriptObject;
        private TemplateContext _globalTemplateContext;

        private struct AdapterInfo
        {
            public string IPAddress;
            public int Port;
        }

        public SHDR(Machine machine, object cfg) : base(machine, cfg)
        {
            _config = cfg;
        }

        private static List<string> GetAllLocalIPv4()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                //.Where(x => x.NetworkInterfaceType == type && x.OperationalStatus == OperationalStatus.Up)
                .Where(x => x.OperationalStatus == OperationalStatus.Up)
                .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(x => x.Address.ToString())
                .ToList();
        }
        
        public override async Task<dynamic?> CreateAsync()
        {
            _adapter = new ShdrAdapter(
                _config.transport["device_name"],
                _config.transport["net"]["port"],
                _config.transport["net"]["heartbeat_ms"]);

            _adapterInfo = new AdapterInfo()
            {
                IPAddress = string.Join(';', GetAllLocalIPv4()),
                Port = _config.transport["net"]["port"]
            };
            
            _adapter.Start();

            _globalScriptObject = new ScriptObject();
            
            _globalScriptObject.Import("machinePaths", 
                new Func<object> (() =>
                {
                    return _machineStructure.Keys.ToArray();
                }));
            
            _globalScriptObject.Import("machineAxisNamesForPath", 
                new Func<object,object> ((p) =>
                {
                    return _machineStructure[p.ToString()].Item1.ToArray();
                }));
            
            _globalScriptObject.Import("machineSpindleNamesForPath", 
                new Func<object,object> ((p) =>
                {
                    return _machineStructure[p.ToString()].Item2.ToArray();
                }));
            
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
            
            _globalScriptObject.Import("ShdrEventIf", 
                new Action<string,object,object,object> ((k,x,y,z) =>
                {
                    _adapter.AddDataItem(k, 
                        Convert.ToBoolean(x) ? y : z); 
                }));
            
            _globalScriptObject.Import("ShdrEventUnavailable", 
                new Action<string> ((k) =>
                {
                    _adapter.AddDataItem(k,"UNAVAILABLE"); 
                }));
            
            _globalScriptObject.Import("ShdrConditionNormal", 
                new Action<string> ((k) =>
                {
                    var c = new ShdrCondition(k, ConditionLevel.NORMAL);
                    _adapter.AddCondition(c); 
                }));
            
            _globalScriptObject.Import("ShdrConditionWarning", 
                new Action<string> ((k) =>
                {
                    var c = new ShdrCondition(k, ConditionLevel.WARNING);
                    _adapter.AddCondition(c); 
                }));
            
            _globalScriptObject.Import("ShdrConditionFault", 
                new Action<string> ((k) =>
                {
                    var c = new ShdrCondition(k, ConditionLevel.FAULT);
                    _adapter.AddCondition(c); 
                }));
            
            _globalScriptObject.Import("ShdrConditionFaultIf", 
                new Action<string,object> ((k,v) =>
                {
                    _adapter.AddCondition(new ShdrCondition(k, 
                        Convert.ToBoolean(v) ? ConditionLevel.FAULT : ConditionLevel.NORMAL)); 
                }));
            
            _globalScriptObject.Import("ShdrConditionSeverity", 
                new Action<string,object,object,object> ((k,f,w,n) =>
                {
                    var c = Convert.ToBoolean(f) ? ConditionLevel.FAULT :
                        Convert.ToBoolean(w) ? ConditionLevel.WARNING :
                        Convert.ToBoolean(n) ? ConditionLevel.NORMAL :
                        ConditionLevel.UNAVAILABLE;
                    _adapter.AddCondition(new ShdrCondition(k, c)); 
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

            _globalScriptObject.SetValue("machine", this.machine, true);
            _globalScriptObject.SetValue("adapter", this._adapterInfo, true);
            
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