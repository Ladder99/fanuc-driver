using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using l99.driver.@base;
using MoreLinq;
using MTConnect.Adapters.Shdr;
using MTConnect.Streams;
using Scriban;
using Scriban.Runtime;

namespace l99.driver.fanuc.transports
{
    public class SHDR : Transport
    {
        private dynamic _config;

        // paths,axes,spindles received from strategy
        private dynamic _model;
        
        // config - veneer type, template text
        private Dictionary<string, string> _transformLookup = new Dictionary<string, string>();
        
        private ShdrAdapter _adapter;
        private Dictionary<string, ShdrDataItem> _cacheShdrDataItems;
        private Dictionary<string, ShdrCondition> _cacheShdrConditions;
        
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

        private void cacheShdrDataItem(ShdrDataItem dataItem)
        {
            Console.WriteLine($"{dataItem.Key}:{dataItem.Value}");
            if (_cacheShdrDataItems.ContainsKey(dataItem.Key))
            {
                _cacheShdrDataItems[dataItem.Key] = dataItem;
            }
            else
            {
                _cacheShdrDataItems.Add(dataItem.Key, dataItem);
            }
        }
        
        private void cacheShdrCondition(ShdrCondition dataItem)
        {
            Console.WriteLine($"{dataItem.Key}:{dataItem.Level}");
            if (_cacheShdrConditions.ContainsKey(dataItem.Key))
            {
                _cacheShdrConditions[dataItem.Key] = dataItem;
            }
            else
            {
                _cacheShdrConditions.Add(dataItem.Key, dataItem);
            }
        }
        
        public override async Task<dynamic?> CreateAsync()
        {
            await initAdapterAsync();

            await initScriptContextAsync();
            
            _transformLookup = (_config.transport["transformers"] as Dictionary<dynamic,dynamic>)
                .ToDictionary(
                    kv => (string)kv.Key, 
                    kv => (string)kv.Value);
            
            return null;
        }

        public override async Task ConnectAsync()
        {
            _adapter.Start();
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
                        try
                        {
                            _globalScriptObject.SetValue("observation", data.observation, true);
                            _globalScriptObject.SetValue("data", data.state.data, true);
                            await Template.EvaluateAsync(_transformLookup[transformName], _globalTemplateContext);
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex, $@"[{{machine.Id}} SHDR evaluation failed for '{transformName}'");
                        }
                    }
                    
                    break;
                
                case "SWEEP_END":

                    if (_transformLookup.ContainsKey("SWEEP_END"))
                    {
                        try
                        {
                            _globalScriptObject.SetValue("observation", data.observation, true);
                            _globalScriptObject.SetValue("data", data.state.data, true);
                            await Template.EvaluateAsync(_transformLookup["SWEEP_END"], _globalTemplateContext);
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex, $@"[{{machine.Id}} SHDR evaluation failed for 'SWEEP_END'");
                        }
                    }
                    
                    _cacheShdrDataItems
                        .ForEach(di => _adapter.AddDataItem(di.Value));
                    _cacheShdrConditions
                        .ForEach(di => _adapter.AddCondition(di.Value));
                    
                    //_adapter.AddConditions(_cacheShdrConditions.Values);
                    //_adapter.AddDataItems(_cacheShdrDataItems.Values);
                    
                    break;
            }
        }

        public override async Task OnGenerateIntermediateModelAsync(dynamic model)
        {
            _model = model;
        }
        
        private async Task initAdapterAsync()
        {
            _cacheShdrDataItems = new Dictionary<string, ShdrDataItem>();
            _cacheShdrConditions = new Dictionary<string, ShdrCondition>();
            
            _adapter = new ShdrAdapter(
                _config.transport["device_name"],
                _config.transport["net"]["port"],
                _config.transport["net"]["heartbeat_ms"]);
            
            _adapter.SendError = (sender, args) =>
            {
                Console.WriteLine(args.Message);
            };

            _adapter.LineSent = (sender, args) =>
            {
                Console.WriteLine(args.Message);
            };

            await ConnectAsync();
        }
        
        private async Task initScriptContextAsync()
        {
            _globalScriptObject = new ScriptObject();
            
            _globalScriptObject.Import("machinePaths", 
                new Func<object> (() =>
                {
                    return _model.structure.Keys.ToArray();
                }));
            
            _globalScriptObject.Import("machineAxisNamesForPath", 
                new Func<object,object> ((p) =>
                {
                    return _model.structure[p.ToString()].Item1.ToArray();
                }));
            
            _globalScriptObject.Import("machineSpindleNamesForPath", 
                new Func<object,object> ((p) =>
                {
                    return _model.structure[p.ToString()].Item2.ToArray();
                }));
            
            _globalScriptObject.Import("ShdrSample", 
                new Action<string,object> ((k,v) =>
                {
                    cacheShdrDataItem(new ShdrDataItem(k,v));
                }));
            
            _globalScriptObject.Import("ShdrSampleUnavailable", 
                new Action<string> ((k) =>
                {
                    cacheShdrDataItem(new ShdrDataItem(k,"UNAVAILABLE"));
                }));
            
            _globalScriptObject.Import("ShdrEvent", 
                new Action<string,object> ((k,v) =>
                {
                    cacheShdrDataItem(new ShdrDataItem(k,v));
                }));
            
            _globalScriptObject.Import("ShdrEventIf", 
                new Action<string,object,object,object> ((k,x,y,z) =>
                {
                    var v = Convert.ToBoolean(x) ? y : z;
                    cacheShdrDataItem(new ShdrDataItem(k,v));
                }));
            
            _globalScriptObject.Import("ShdrEventUnavailable", 
                new Action<string> ((k) =>
                {
                    cacheShdrDataItem(new ShdrDataItem(k,"UNAVAILABLE"));
                }));
            
            _globalScriptObject.Import("ShdrConditionNormal", 
                new Action<string> ((k) =>
                {
                    var c = new ShdrCondition(k, ConditionLevel.NORMAL);
                    cacheShdrCondition(c);
                }));
            
            _globalScriptObject.Import("ShdrConditionWarning", 
                new Action<string> ((k) =>
                {
                    var c = new ShdrCondition(k, ConditionLevel.WARNING);
                    cacheShdrCondition(c); 
                }));
            
            _globalScriptObject.Import("ShdrConditionFault", 
                new Action<string> ((k) =>
                {
                    var c = new ShdrCondition(k, ConditionLevel.FAULT);
                    cacheShdrCondition(c);
                }));
            
            _globalScriptObject.Import("ShdrConditionFaultIf", 
                new Action<string,object> ((k,v) =>
                {
                    var c = new ShdrCondition(k, 
                        Convert.ToBoolean(v) ? ConditionLevel.FAULT : ConditionLevel.NORMAL);
                    cacheShdrCondition(c);
                }));
            
            _globalScriptObject.Import("ShdrConditionSeverity", 
                new Action<string,object,object,object> ((k,f,w,n) =>
                {
                    var l = Convert.ToBoolean(f) ? ConditionLevel.FAULT :
                        Convert.ToBoolean(w) ? ConditionLevel.WARNING :
                        Convert.ToBoolean(n) ? ConditionLevel.NORMAL :
                        ConditionLevel.UNAVAILABLE;
                    var c = new ShdrCondition(k, l);
                    cacheShdrCondition(c);
                }));
            
            _globalScriptObject.Import("ShdrConditionUnavailable", 
                new Action<string> ((k) =>
                {
                    var c = new ShdrCondition(k, ConditionLevel.UNAVAILABLE);
                    cacheShdrCondition(c);
                }));
            
            /*
            _globalScriptObject.Import("ShdrAllUnavailable", 
                new Action (() =>
                {
                    _adapter.SetUnavailable(); 
                }));
            */
            
            _globalScriptObject.SetValue("machine", this.machine, true);
            _globalScriptObject.SetValue("adapter", 
                new AdapterInfo()
                {
                    IPAddress = string.Join(';', this.getAllLocalIPv4()),
                    Port = _config.transport["net"]["port"]
                }, true);
            
            _globalTemplateContext = new TemplateContext();
            _globalTemplateContext.PushGlobal(_globalScriptObject);
        }
        
        private List<string> getAllLocalIPv4()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                //.Where(x => x.NetworkInterfaceType == type && x.OperationalStatus == OperationalStatus.Up)
                .Where(x => x.OperationalStatus == OperationalStatus.Up)
                .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(x => x.Address.ToString())
                .ToList();
        }
    }
}