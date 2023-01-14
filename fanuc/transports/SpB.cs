#pragma warning disable CS1998

using l99.driver.@base;
using SparkplugNet.Core.Node;
using SparkplugNet.VersionB;
using SparkplugNet.VersionB.Data;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.transports;
public class SpB : Transport
{
    private enum DeviceStateEnum
    {
        Offline,
        Online,
        MustRebirth
    }
    
    private readonly dynamic _config;

    private Dictionary<string, dynamic> _previous = new();
    private Dictionary<string, dynamic> _current = new();

    private SparkplugNode _node = null!;
    private SparkplugNodeOptions _nodeOptions = null!;
    private List<Metric> _nodeMetrics = null!;
    
    private DeviceStateEnum _deviceState = DeviceStateEnum.Offline;
    
    public SpB(Machine machine, object cfg) : base(machine, cfg)
    {
        _config = cfg;
    }

    public override async Task<dynamic?> CreateAsync()
    {
        _nodeMetrics = new List<Metric>()
        {
            new()
            {
                Name = "IpAddress", DataType = DataType.String, StringValue = String.Join(';', Network.GetAllLocalIPv4())
            }
        };

        // ReSharper disable once RedundantArgumentDefaultValue
        _node = new SparkplugNode(_nodeMetrics, null);

        // ReSharper disable once UseObjectOrCollectionInitializer
        _nodeOptions = new SparkplugNodeOptions(
            brokerAddress: _config.transport["net"]["ip"],
            port: _config.transport["net"]["port"],
            userName: _config.transport["user"],
            clientId: $"fanuc_{machine.Id}",
            password: _config.transport["password"],
            useTls: false,
            scadaHostIdentifier: "scada",
            groupIdentifier: $"fanuc",
            edgeNodeIdentifier: $"{Environment.MachineName}_{machine.Id}",    // TODO: clean up hostname to spb spec
            reconnectInterval: TimeSpan.FromSeconds(30),
            webSocketParameters: null,
            proxyOptions: null,
            cancellationToken: new CancellationToken()
        );

        _nodeOptions.AddSessionNumberToDataMessages = true;
        
        // ReSharper disable once UnusedParameter.Local
        _node.DisconnectedAsync += async args => 
        {
            Logger.Warn($"[{machine.Id}] SpB node disconnected.");
        };
        
        // ReSharper disable once UnusedParameter.Local
        _node.NodeCommandReceivedAsync += async args =>   
        {
            Logger.Info($"[{machine.Id}] SpB node incoming command.");
        }; 
        
        _node.StatusMessageReceivedAsync += async args =>
        {
            Logger.Warn($"[{machine.Id}] SpB node status message '{args.Status}.");
        };

        await ConnectAsync();
        return null;
    }

    public override async Task ConnectAsync()
    {
        if (_config.machine.enabled)
        {
            if (!_node.IsConnected)
            {
                try
                {
                    await _node.Start(_nodeOptions);
                    await _node.PublishMetrics(_nodeMetrics);
                    Logger.Info($"[{machine.Id}] SpB node connected.");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"[{machine.Id}] Broker connection error.");
                }
            }
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

                var prefix = $"{veneer.Name}{(veneer.SliceKey == null ? null : "/"+veneer.SliceKey)}";
                ProcessIncoming(prefix, data);
                
                break;
                
            case "SWEEP_END":
                
                ProcessIncoming("sweep", data);

                var changes = _current
                    .Except(_previous)
                    .ToDictionary();

                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace($"[{machine.Id}] Total metric count: {_current.Count()}");
                    Logger.Trace($"[{machine.Id}] Changed metric count: {changes.Count()}");
                    changes.ForEach(x =>
                    {
                        Logger.Trace($"[{machine.Id}] Metric change: {x.Key} = {x.Value.ToString()}");
                    });
                }

                _previous = _current
                    .ToDictionary(
                        kvp => kvp.Key, 
                        kvp => kvp.Value);
                
                if (data.state.data.online == true && _deviceState == DeviceStateEnum.Offline)
                {
                    Logger.Info($"[{machine.Id}] SpB device birth.");
                    bool success = await DeviceBirthAsync(_current);
                    _deviceState = success ? DeviceStateEnum.Online : DeviceStateEnum.MustRebirth;
                }
                else if (data.state.data.online == false && _deviceState == DeviceStateEnum.Online)
                {
                    Logger.Info($"[{machine.Id}] SpB device death.");
                    await DeviceDeathAsync();
                    _deviceState = DeviceStateEnum.Offline;
                }
                else if (data.state.data.online == true && _deviceState == DeviceStateEnum.MustRebirth)
                {
                    Logger.Info($"[{machine.Id}] SpB device re-birth.");
                    bool success = await DeviceBirthAsync(_current);
                    if (success)
                    {
                        _deviceState = DeviceStateEnum.Online;
                    }
                }
                else
                {
                    await DeviceUpdateAsync(changes);
                }
                
                break;
            
            case "INT_MODEL":

                break;
        }
    }

    void ProcessIncoming(string prefix, dynamic data)
    {
        // flatten incoming data
        JObject jc = JObject.FromObject(data.state.data);
        var fc = jc.Flatten();
        
        // massage keys
        var dict = fc.Select(x=>
            {
                // ReSharper disable once ConvertToLambdaExpression
                return new KeyValuePair<string, object>(
                    //$"{prefix.Replace('/','.')}.{x.Key.Replace('[','_').Replace("]", string.Empty)}",
                    //$"{prefix.Replace('/','.')}.{x.Key}",
                    $"{prefix}/{x.Key.Replace('.','/')}",
                    x.Value);
            })
            .ToDictionary(
                x=> x.Key,
                x=> x.Value);
           
        // remove arrays for now, we need to deal with those differently
        var keys = dict.Keys.Where(k => k.Contains('[')).ToList();
        foreach (var key in keys)
        {
            dict.Remove(key);
        }
        
        // update current with incoming changes
        _current = dict
            .Concat(_current)
            .GroupBy(e => e.Key)
            .ToDictionary(g => g.Key, g => g.First().Value);
    }

    private async Task<bool> DeviceBirthAsync(Dictionary<string, object> diff)
    {
        List<Metric> metrics = diff
            .Select(kv => MakeMetric(kv.Key, kv.Value))
            .ToList();

        if (_node.IsConnected)
        {
            try
            {
                await _node.PublishDeviceBirthMessage(metrics, machine.Id);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"[{machine.Id}] SpB device birth error.");
                return false;
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    private async Task DeviceDeathAsync()
    {
        if (_node.IsConnected)
        {
            try
            {
                await _node.PublishDeviceDeathMessage(machine.Id);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"[{machine.Id}] SpB device death error.");
            }
        }
    }

    private async Task DeviceUpdateAsync(Dictionary<string, object> diff)
    {
        List<Metric> metrics = diff
            .Select(kv => MakeMetric(kv.Key, kv.Value))
            .ToList();

        if (_node.IsConnected)
        {
            try
            {
                await _node.PublishDeviceData(metrics, machine.Id);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"[{machine.Id}] SpB device data error.");
            }
        }
    }

    private Metric MakeMetric(string name, object value)
    {
        switch (Type.GetTypeCode(value.GetType()))
        {
            case TypeCode.Byte:
                return new Metric()
                {
                    Name = name, DataType = DataType.Int8, IntValue = Convert.ToUInt16(value)
                };
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Double:
                return new Metric()
                {
                    Name = name, DataType = DataType.Double, DoubleValue = Convert.ToDouble(value) 
                };
            case TypeCode.Boolean:
                return new Metric()
                {
                    Name = name, DataType = DataType.Boolean, BooleanValue = Convert.ToBoolean(value) 
                };
            case TypeCode.String:
                return new Metric()
                {
                    Name = name, DataType = DataType.String, StringValue = Convert.ToString(value) ?? string.Empty 
                };
            default:
                Logger.Info($"[{machine.Id}] '{name}'({value.GetType().FullName}) converted to string metric.");
                
                return new Metric()
                {
                    Name = name, DataType = DataType.String, StringValue = Convert.ToString(value) ?? string.Empty 
                };
        }
    }
}
#pragma warning restore CS1998