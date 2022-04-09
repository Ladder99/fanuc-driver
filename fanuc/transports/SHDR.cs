using l99.driver.@base;
using MTConnect.Adapters.Shdr;
using MTConnect.Observations;
using Scriban;
using Scriban.Runtime;

namespace l99.driver.fanuc.transports;

public class MTCDeviceModelGenerator
{
    private ILogger _logger;
    private Machine _machine;
    private dynamic _transport;
    
    public MTCDeviceModelGenerator(Machine machine, dynamic transport)
    {
        _logger = LogManager.GetLogger(this.GetType().FullName);
        _machine = machine;
        _transport = transport;
    }
    
    public void Generate(dynamic model)
    {
        if (!_transport["generator"]["enabled"])
            return;
        
        try
        {
            var generator = _transport["generator"];

            Template tp = null;
            var so = new ScriptObject();
            var tc = new TemplateContext();

            var paths = model.structure.Keys;

            var axes = new Dictionary<string, List<string>>();
            var spindles = new Dictionary<string, List<string>>();

            foreach (var path in model.structure.Keys)
            {
                axes.Add(path, model.structure[path].Item1);
                spindles.Add(path, model.structure[path].Item2);
            }

            so.Import("GenerateAxis",
                new Func<string, string, string, object>((section, path, axis) =>
                {
                    so.SetValue("path", path, true);
                    so.SetValue("axis", axis, true);
                    so.SetValue("spindle", null, true);
                    tp = Template.Parse(section);
                    var x = tp.Render(tc);
                    so.SetValue("path", null, true);
                    so.SetValue("axis", null, true);
                    return x;
                }));

            so.Import("GenerateSpindle",
                new Func<string, string, string, object>((section, path, spindle) =>
                {
                    so.SetValue("path", path, true);
                    so.SetValue("axis", null, true);
                    so.SetValue("spindle", spindle, true);
                    tp = Template.Parse(section);
                    var x = tp.Render(tc);
                    so.SetValue("path", null, true);
                    so.SetValue("spindle", null, true);
                    return x;
                }));

            so.Import("GeneratePath",
                new Func<string, string, object>((section, path) =>
                {
                    so.SetValue("path", path, true);
                    so.SetValue("axis", null, true);
                    so.SetValue("spindle", null, true);
                    tp = Template.Parse(section);
                    var x = tp.Render(tc);
                    so.SetValue("path", null, true);
                    return x;
                }));

            tc.PushGlobal(so);

            so.SetValue("generator", generator, true);
            so.SetValue("device", _transport["device_name"], true);
            so.SetValue("paths", paths, true);
            so.SetValue("axes", axes, true);
            so.SetValue("spindles", spindles, true);
            tp = Template.Parse(generator["root"]);
            var xml = tp.Render(tc);

            tp = Template.Parse(generator["output"]);
            var file_out = tp.Render(tc);

            System.IO.File.WriteAllText(file_out, xml);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"[{_machine.Id} MTC device model generation failed!" );
        }
    }
}

public class SHDR : Transport
{
    private dynamic _config;

    // paths,axes,spindles received from strategy
    private dynamic _model;
    
    // config - veneer type, template text
    private Dictionary<string, string> _transformLookup = new Dictionary<string, string>();
    
    private ShdrAdapter _adapter;
    private bool _shdrInvalidated = false;

    private bool shdrInvalidated
    {
        get
        {
            bool flag = _shdrInvalidated;
            _shdrInvalidated = false;
            return flag;
        }
    }
    
    private ScriptObject _globalScriptObject;
    private TemplateContext _globalTemplateContext;

    private MTCDeviceModelGenerator _deviceModelGenerator;

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
        _adapter.AddDataItem(dataItem);
        //Console.WriteLine($"{dataItem.DataItemKey}:{string.Join(',', dataItem.Values.Select(v=>v.Value))}");
    }
    
    private void cacheShdrMessage(ShdrMessage dataItem)
    {
        _adapter.AddMessage(dataItem);
        //Console.WriteLine($"{dataItem.DataItemKey}:{string.Join(',', dataItem.Values.Select(v=>v.Value))}");
    }
    
    private void cacheShdrCondition(ShdrCondition dataItem)
    {
        _adapter.AddCondition(dataItem);
        //Console.WriteLine($"{dataItem.DataItemKey}:{string.Join(',', dataItem.Values.Select(v=>v.Value))}");
    }
    
    public override async Task<dynamic?> CreateAsync()
    {
        _deviceModelGenerator = new MTCDeviceModelGenerator(machine, _config.transport);
        
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
        if (_config.machine.enabled)
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
                        logger.Warn(ex, $"[{machine.Id}] SHDR evaluation failed for '{transformName}'");
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
                        logger.Warn(ex, $"[{machine.Id}] SHDR evaluation failed for 'SWEEP_END'");
                    }
                }
                
                if (shdrInvalidated)
                {
                    _adapter.SetUnavailable();
                }

                break;
        }
    }

    public override async Task OnGenerateIntermediateModelAsync(dynamic model)
    {
        _model = model;
        _deviceModelGenerator.Generate(model);
    }
    
    private async Task initAdapterAsync()
    {
        _adapter = new ShdrAdapter(
            _config.transport["device_name"],
            _config.transport["net"]["port"],
            _config.transport["net"]["heartbeat_ms"]);

        _adapter.Interval = _config.transport["net"]["interval_ms"];
        
        _adapter.AgentConnectionError = (sender, s) =>
        {
            logger.Info($"[{machine.Id}] MTC Agent connection error. {s}");
        };
        
        _adapter.AgentDisconnected = (sender, s) =>
        {
            logger.Info($"[{machine.Id}] MTC Agent disconnected error. {s}");
        };
        
        _adapter.AgentConnected = (sender, s) =>
        {
            logger.Info($"[{machine.Id}] MTC Agent connected. {s}");
        };
        
        _adapter.SendError = (sender, args) =>
        {
            logger.Info($"[{machine.Id}] MTC Agent send error. {args.Message}");
        };

        _adapter.LineSent = (sender, args) =>
        {
            logger.Debug($"[{machine.Id}] MTC Agent line send. {args.Message}");
        };

        _adapter.PingReceived = (sender, s) =>
        {
            //logger.Info($"[{machine.Id} MTC Agent ping received. {s}");
        };

        _adapter.PongSent = (sender, s) =>
        {
            //logger.Info($"[{machine.Id} MTC Agent pong sent. {s}");
        };

        await ConnectAsync();
    }

    private async Task initScriptContextAsync()
    {
        _globalScriptObject = new ScriptObject();

        _globalScriptObject.Import("machinePaths",
            new Func<object>(() => { return _model.structure.Keys.ToArray(); }));

        _globalScriptObject.Import("machineAxisNamesForPath",
            new Func<object, object>((p) => { return _model.structure[p.ToString()].Item1.ToArray(); }));

        _globalScriptObject.Import("machineSpindleNamesForPath",
            new Func<object, object>((p) => { return _model.structure[p.ToString()].Item2.ToArray(); }));

        _globalScriptObject.Import("ShdrSample",
            new Action<string, object>((k, v) => { cacheShdrDataItem(new ShdrDataItem(k, v)); }));

        _globalScriptObject.Import("ShdrSampleUnavailable",
            new Action<string>((k) => { cacheShdrDataItem(new ShdrDataItem(k, "UNAVAILABLE")); }));

        _globalScriptObject.Import("ShdrMessage",
            new Action<string, object>((k, v) => { cacheShdrMessage(new ShdrMessage(k, v)); }));

        _globalScriptObject.Import("ShdrEvent",
            new Action<string, object>((k, v) => { cacheShdrDataItem(new ShdrDataItem(k, v)); }));

        _globalScriptObject.Import("ShdrEventIf",
            new Action<string, object, object, object>((k, x, y, z) =>
            {
                var v = Convert.ToBoolean(x) ? y : z;
                cacheShdrDataItem(new ShdrDataItem(k, v));
            }));

        _globalScriptObject.Import("ShdrEventUnavailable",
            new Action<string>((k) => { cacheShdrDataItem(new ShdrDataItem(k, "UNAVAILABLE")); }));

        _globalScriptObject.Import("ShdrConditionNormal",
            new Action<string>((k) =>
            {
                var c = new ShdrCondition(k, ConditionLevel.NORMAL);
                cacheShdrCondition(c);
            }));

        _globalScriptObject.Import("ShdrConditionWarning",
            new Action<string>((k) =>
            {
                var c = new ShdrCondition(k, ConditionLevel.WARNING);
                cacheShdrCondition(c);
            }));

        _globalScriptObject.Import("ShdrConditionFault",
            new Action<string>((k) =>
            {
                var c = new ShdrCondition(k, ConditionLevel.FAULT);
                cacheShdrCondition(c);
            }));

        _globalScriptObject.Import("ShdrConditionFaultIf",
            new Action<string, object>((k, v) =>
            {
                var c = new ShdrCondition(k,
                    Convert.ToBoolean(v) ? ConditionLevel.FAULT : ConditionLevel.NORMAL);
                cacheShdrCondition(c);
            }));

        _globalScriptObject.Import("ShdrConditionSeverity",
            new Action<string, object, object, object>((k, f, w, n) =>
            {
                var l = Convert.ToBoolean(f) ? ConditionLevel.FAULT :
                    Convert.ToBoolean(w) ? ConditionLevel.WARNING :
                    Convert.ToBoolean(n) ? ConditionLevel.NORMAL :
                    ConditionLevel.UNAVAILABLE;
                var c = new ShdrCondition(k, l);
                cacheShdrCondition(c);
            }));

        _globalScriptObject.Import("ShdrConditionUnavailable",
            new Action<string>((k) =>
            {
                var c = new ShdrCondition(k, ConditionLevel.UNAVAILABLE);
                cacheShdrCondition(c);
            }));

        _globalScriptObject.Import("ShdrAllUnavailable",
            new Action(() => { _shdrInvalidated = true; }));

        _globalScriptObject.SetValue("machine", this.machine, true);
        _globalScriptObject.SetValue("device", _config.transport["device_name"], true);
        _globalScriptObject.SetValue("adapter",
            new AdapterInfo()
            {
                IPAddress = string.Join(';', Network.GetAllLocalIPv4()),
                Port = _config.transport["net"]["port"]
            }, true);

        _globalTemplateContext = new TemplateContext();
        _globalTemplateContext.PushGlobal(_globalScriptObject);
    }
}