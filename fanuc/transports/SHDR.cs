using l99.driver.@base;
using l99.driver.fanuc.utils;
using MTConnect.Adapters.Shdr;
using MTConnect.Observations;
using MTConnect.Shdr;
using Scriban;
using Scriban.Runtime;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.transports;

// ReSharper disable once InconsistentNaming
// ReSharper disable once UnusedType.Global
public class SHDR : Transport
{
    // SHDR adapter support
    private ShdrQueueAdapter _adapter = null!;
    private Dictionary<string, ShdrDataItem> _cacheShdrDataItems = new();
    private Dictionary<string, ShdrMessage> _cacheShdrMessages = new();
    private Dictionary<string, ShdrCondition> _cacheShdrConditions = new();
    private bool _shdrInvalidated;
    
    // Scriban support
    private ScriptObject _globalScriptObject = null!;
    private TemplateContext _globalTemplateContext = null!;
    private Dictionary<string, object> _scriptCache = new();

    private MTCDeviceModelGenerator _deviceModelGenerator = null!;
    // paths,axes,spindles received from strategy
    private dynamic _model = null!;
    
    // config - veneer type, template text
    private Dictionary<string, string> _transformLookup = new();

    public SHDR(Machine machine) : base(machine)
    {
        if (!Machine.Configuration.transport.ContainsKey("transformers"))
        {
            Logger.Error($"[{Machine.Id}] Transformers must be explicitly defined in configuration.");
            Machine.Disable();
        }
         
        if (!Machine.Configuration.transport.ContainsKey("generator"))
        {
            Logger.Error($"[{Machine.Id}] Generator must be explicitly defined in configuration.");
            Machine.Disable();
        }
        
        if (!Machine.Configuration.transport.ContainsKey("device_key"))
            Machine.Configuration.transport.Add("device_key", Machine.Id);
        
        if (!Machine.Configuration.transport.ContainsKey("device_name"))
            Machine.Configuration.transport.Add("device_name", Machine.Id);
        
        if (!Machine.Configuration.transport.ContainsKey("net"))
        {
            Machine.Configuration.transport.Add("net", new Dictionary<object, object>()
            {
                { "filter_duplicates", true },
                { "heartbeat_ms", 10000 },
                { "port", 7878 }
            });
        }
        else if (Machine.Configuration.transport["net"] == null)
        {
            Machine.Configuration.transport["net"] = new Dictionary<object, object>()
            {
                { "filter_duplicates", true },
                { "heartbeat_ms", 10000 },
                { "port", 7878 }
            };
        }
    }

    private bool ShdrInvalidated
    {
        get
        {
            var flag = _shdrInvalidated;
            _shdrInvalidated = false;
            return flag;
        }
    }

    private void CacheShdrDataItem(ShdrDataItem dataItem)
    {
        _cacheShdrDataItems[dataItem.DataItemKey] = dataItem;
        Logger.Trace($"[{Machine.Id}] (CacheShdrDataItem) {dataItem.DataItemKey}:{string.Join(',', dataItem.Values.Select(v => v.Value))}");
    }

    private void CacheShdrMessage(ShdrMessage dataItem)
    {
        _cacheShdrMessages[dataItem.DataItemKey] = dataItem;
        Logger.Trace($"[{Machine.Id}] (CacheShdrMessage) {dataItem.DataItemKey}:{string.Join(',', dataItem.Values.Select(v => v.Value))}");
    }

    private void CacheShdrCondition(ShdrCondition dataItem)
    {
         _cacheShdrConditions[dataItem.DataItemKey] = dataItem;
        Logger.Trace(
            $"[{Machine.Id}] (CacheShdrCondition) {dataItem.DataItemKey}:{string.Join(',', dataItem.FaultStates.Select(v => v.Level))}");
    }

    public override async Task<dynamic?> CreateAsync()
    {
        _deviceModelGenerator = new MTCDeviceModelGenerator(Machine, Machine.Configuration.transport);

        await InitAdapterAsync();

        await InitScriptContextAsync();

        _transformLookup = ((Machine.Configuration.transport["transformers"] as Dictionary<dynamic, dynamic>)!)
            .ToDictionary(
                kv => (string) kv.Key,
                kv => (string) kv.Value);

        return null;
    }

    public override async Task ConnectAsync()
    {
        if (Machine.Enabled)
        {
            Logger.Info($"[{Machine.Id}] Starting ShdrAdapter.");
            _adapter.Start();
        }
    }

    public override async Task SendAsync(params dynamic[] parameters)
    {
        var @event = parameters[0];
        var veneer = parameters[1];
        var data = parameters[2];

        switch (@event)
        {
            case "DATA_ARRIVE":

                var transformName =
                    $"{veneer.GetType().FullName}, {veneer.GetType().Assembly.GetName().Name}";

                if (_transformLookup.TryGetValue(transformName, out var dataArriveExpression))
                    try
                    {
                        _globalScriptObject.SetValue("observation", data.observation, true);
                        _globalScriptObject.SetValue("data", data.state.data, true);
                        await Template.EvaluateAsync(dataArriveExpression, _globalTemplateContext);
                    }
                    catch (Exception ex)
                    { 
                        Logger.Warn(ex, $"[{Machine.Id}] SHDR evaluation failed for '{transformName}'");
                    }

                break;

            case "SWEEP_END":

                if (_transformLookup.TryGetValue("SWEEP_END", out var sweepEndExpression))
                {
                    try
                    {
                        _globalScriptObject.SetValue("observation", data.observation, true);
                        _globalScriptObject.SetValue("data", data.state.data, true);
                        await Template.EvaluateAsync(sweepEndExpression, _globalTemplateContext);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, $"[{Machine.Id}] SHDR evaluation failed for 'SWEEP_END'");
                    }
                }

                // Move cached data items to adapter.
                //  This prevents duplicate observations if multiple Scriban functions
                //  evaluate the same data item at different times within the collection cycle.
                _adapter.AddDataItems(_cacheShdrDataItems.Values);
                _adapter.AddMessages(_cacheShdrMessages.Values);
                _adapter.AddConditions(_cacheShdrConditions.Values);
                _cacheShdrDataItems.Clear();
                _cacheShdrMessages.Clear();
                _cacheShdrConditions.Clear();
                
                // All data can be invalidated by calling ShdrAllUnavailable from Scriban.
                if (ShdrInvalidated) _adapter.SetUnavailable();

                // We are at the end of collection cycle, send the buffer.
                Logger.Trace($"[{Machine.Id}] -> Adapter.SendBuffer");
                _adapter.SendBuffer();
                Logger.Trace($"[{Machine.Id}] Adapter.SendBuffer ->");
                
                break;
        }
    }

    public override async Task OnGenerateIntermediateModelAsync(dynamic model)
    {
        _model = model;
        _deviceModelGenerator.Generate(model);
    }

    private async Task InitAdapterAsync()
    {
        // DataItem evaluation is allowed to overwrite previously 
        //  written data in the buffer.  We want to control when we send the buffer
        //  at the end of each collection cycle.
        Logger.Info($"[{Machine.Id}] Creating ShdrAdapter, " +
                    $"device_key:{Machine.Configuration.transport["device_key"]}, " +
                    $"port:{Machine.Configuration.transport["net"]["port"]}, " +
                    $"heartbeat_ms:{Machine.Configuration.transport["net"]["heartbeat_ms"]}, " +
                    $"filter_duplicates:{Machine.Configuration.transport["net"]["filter_duplicates"]}");
        
        // ReSharper disable once UseObjectOrCollectionInitializer
        _adapter = new ShdrQueueAdapter(
            Machine.Configuration.transport["device_key"],
            Machine.Configuration.transport["net"]["port"],
            Machine.Configuration.transport["net"]["heartbeat_ms"]);
        
        _adapter.FilterDuplicates = Machine.Configuration.transport["net"]["filter_duplicates"];
        
        // ReSharper disable once UnusedParameter.Local
        _adapter.AgentConnectionError += (sender, s) =>
        {
            Logger.Warn($"[{Machine.Id}] MTC Agent connection error. {s}");
        };

        // ReSharper disable once UnusedParameter.Local
        _adapter.AgentDisconnected += (sender, s) =>
        {
            Logger.Warn($"[{Machine.Id}] MTC Agent disconnected error. {s}");
        };

        // ReSharper disable once UnusedParameter.Local
        _adapter.AgentConnected += (sender, s) =>
        {
            Logger.Info($"[{Machine.Id}] MTC Agent connected. {s}");
        };

        // ReSharper disable once UnusedParameter.Local
        _adapter.SendError += (sender, args) =>
        {
            Logger.Warn($"[{Machine.Id}] MTC Agent send error. {args.Message}");
        };

        // ReSharper disable once UnusedParameter.Local
        _adapter.LineSent += (sender, args) =>
        {
            Logger.Debug($"[{Machine.Id}] MTC Agent line send. {args.Message}");
        };
        
        _adapter.PingReceived += (sender, s) =>
        {
            Logger.Debug($"[{Machine.Id}] MTC Agent ping received. {s}");
        };

        // ReSharper disable once UnusedParameter.Local
        _adapter.PongSent += (sender, s) =>
        {
            Logger.Debug($"[{Machine.Id}] MTC Agent pong sent. {s}");
        };

        await ConnectAsync();
    }

    private async Task InitScriptContextAsync()
    {
        _globalScriptObject = new ScriptObject();

        _globalScriptObject.Import("ToDebug", 
            new Action<object>((o) =>
            {
                Console.WriteLine(o);
            }));
        
        _globalScriptObject.Import("ToCache", 
            new Func<string, object, object>((k, o) =>
            {
                _scriptCache[k] = o;
                return o;
            }));
        
        _globalScriptObject.Import("FromCache", 
            new Func<string, object, object>((k, o) =>
            {
                return _scriptCache.TryGetValue(k, out var value) ? value : o;
            }));
        
        _globalScriptObject.Import("GetValue", 
            new Func<object, object, object>((k, o) =>
            {
                var t = o.GetType();
                var isDict = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>);

                if (isDict)
                {
                    return (o as Dictionary<object, object>)[k];
                }

                return null;

            }));
        
        _globalScriptObject.Import("ToArray", 
            new Func<object, object>((o) =>
            {
                var t = o.GetType();
                var isDict = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>);

                if (isDict)
                {
                    return (o as Dictionary<object, object>).Values.ToList();
                }

                return null;

            }));
        
        _globalScriptObject.Import("machinePaths",
            // ReSharper disable once ConvertToLambdaExpression
            new Func<object>(() =>
            {
                return _model.structure.Keys.ToArray();
            }));

        _globalScriptObject.Import("machineAxisNamesForPath",
            // ReSharper disable once ConvertToLambdaExpression
            new Func<object, object>(p =>
            {
                return _model.structure[p.ToString()].Item1.ToArray();
            }));

        _globalScriptObject.Import("machineSpindleNamesForPath",
            // ReSharper disable once ConvertToLambdaExpression
            new Func<object, object>(p =>
            {
                return _model.structure[p.ToString()].Item2.ToArray();
            }));

        _globalScriptObject.Import("ShdrSample",
            new Action<string, object>((k, v) =>
            {
                CacheShdrDataItem(new ShdrDataItem(k, v));
            }));

        _globalScriptObject.Import("ShdrSampleUnavailable",
            new Action<string>(k =>
            {
                CacheShdrDataItem(new ShdrDataItem(k, "UNAVAILABLE"));
            }));

        _globalScriptObject.Import("ShdrMessage",
            new Action<string, object>((k, v) =>
            {
                CacheShdrMessage(new ShdrMessage(k, v.ToString()));
            }));

        _globalScriptObject.Import("ShdrEvent",
            new Action<string, object>((k, v) =>
            {
                CacheShdrDataItem(new ShdrDataItem(k, v));
            }));

        _globalScriptObject.Import("ShdrEventIf",
            new Action<string, object, object, object>((k, x, y, z) =>
            {
                var v = Convert.ToBoolean(x) ? y : z;
                CacheShdrDataItem(new ShdrDataItem(k, v));
            }));

        _globalScriptObject.Import("ShdrEventUnavailable",
            new Action<string>(k =>
            {
                CacheShdrDataItem(new ShdrDataItem(k, "UNAVAILABLE"));
            }));

        _globalScriptObject.Import("ShdrConditionNormal",
            new Action<string, string, string>((key, nativeCode, text) =>
            {
                var c = new ShdrCondition(key);
                if (string.IsNullOrEmpty(nativeCode))
                {
                    c.Normal();
                }
                else
                {
                    c.AddNormal(nativeCode, text);
                }
                CacheShdrCondition(c);
            }));

        _globalScriptObject.Import("ShdrConditionWarning",
            new Action<string, string, string>((key, nativeCode, text) =>
            {
                var c = new ShdrCondition(key);
                if (string.IsNullOrEmpty(nativeCode))
                {
                    c.Warning();
                }
                else
                {
                    c.AddWarning(text, nativeCode);
                }
                CacheShdrCondition(c);
            }));
        
        _globalScriptObject.Import("ShdrConditionWarningIf",
            new Action<string, object, string, string>((key, value, nativeCode, text) =>
            {
                var c = new ShdrCondition(key);
                var makeWarn = Convert.ToBoolean(value);
                if (string.IsNullOrEmpty(nativeCode))
                {
                    if (makeWarn)
                    {
                        c.Warning();
                    }
                    else
                    {
                        c.Normal();
                    }
                }
                else
                {
                    if (makeWarn)
                    {
                        c.AddWarning(text, nativeCode);
                    }
                    else
                    {
                        c.AddNormal(nativeCode, text);
                    }
                }
                CacheShdrCondition(c);
            }));

        _globalScriptObject.Import("ShdrConditionFault",
            new Action<string, string, string>((key, nativeCode, text) =>
            {
                var c = new ShdrCondition(key);
                if (string.IsNullOrEmpty(nativeCode))
                {
                    c.Fault();
                }
                else
                {
                    c.AddFault(text, nativeCode);
                }
                CacheShdrCondition(c);
            }));

        _globalScriptObject.Import("ShdrConditionFaultIf",
            new Action<string, object, string, string>((key, value, nativeCode, text) =>
            {
                var c = new ShdrCondition(key);
                var makeFault = Convert.ToBoolean(value);
                if (string.IsNullOrEmpty(nativeCode))
                {
                    if (makeFault)
                    {
                        c.Fault();
                    }
                    else
                    {
                        c.Normal();
                    }
                }
                else
                {
                    if (makeFault)
                    {
                        c.AddFault(text, nativeCode);
                    }
                    else
                    {
                        c.AddNormal(nativeCode, text);
                    }
                }
                CacheShdrCondition(c);
            }));

        _globalScriptObject.Import("ShdrConditionSeverity",
            new Action<string, object, object, object>((key, isFault, isWarning, isNormal) =>
            {
                var level = Convert.ToBoolean(isFault) ? ConditionLevel.FAULT :
                    Convert.ToBoolean(isWarning) ? ConditionLevel.WARNING :
                    Convert.ToBoolean(isNormal) ? ConditionLevel.NORMAL :
                    ConditionLevel.UNAVAILABLE;
                var c = new ShdrCondition(key, level);
                CacheShdrCondition(c);
            }));

        _globalScriptObject.Import("ShdrConditionUnavailable",
            new Action<string>(k =>
            {
                var c = new ShdrCondition(k, ConditionLevel.UNAVAILABLE);
                CacheShdrCondition(c);
            }));

        _globalScriptObject.Import("ShdrAllUnavailable",
            new Action(() => { _shdrInvalidated = true; }));

        _globalScriptObject.SetValue("machine", Machine, true);
        _globalScriptObject.SetValue("device", Machine.Configuration.transport["device_name"], true);
        _globalScriptObject.SetValue("adapter",
            new AdapterInfo
            {
                IPAddress = string.Join(';', Network.GetAllLocalIPv4()),
                Port = Machine.Configuration.transport["net"]["port"]
            }, true);

        _globalTemplateContext = new TemplateContext();
        _globalTemplateContext.PushGlobal(_globalScriptObject);
    }

    private struct AdapterInfo
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once NotAccessedField.Local
        public string IPAddress;

        // ReSharper disable once NotAccessedField.Local
        public int Port;
    }
}