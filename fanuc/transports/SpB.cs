using l99.driver.@base;
using SparkplugNet.Core.Node;
using SparkplugNet.VersionB;
using SparkplugNet.VersionB.Data;

namespace l99.driver.fanuc.transports;
public class SpB : Transport
{
    private enum DeviceStateEnum
    {
        OFFLINE,
        ONLINE
    }
    
    private dynamic _config;

    private Dictionary<string, dynamic> _previous = new Dictionary<string, dynamic>();
    private Dictionary<string, dynamic> _current = new Dictionary<string, dynamic>();

    private SparkplugNode _node;
    private SparkplugNodeOptions _nodeOptions;
    private List<Metric> _nodeMetrics;
    
    private DeviceStateEnum _deviceState = DeviceStateEnum.OFFLINE;
    
    public SpB(Machine machine, object cfg) : base(machine, cfg)
    {
        _config = cfg;
    }

    public override async Task<dynamic?> CreateAsync()
    {
        List<Metric> metrics = new List<Metric>()
        {
            new()
            {
                Name = "IpAddress", DataType = (uint)DataType.String, StringValue = String.Join(';', Network.GetAllLocalIPv4())
            }
        };

        _node = new SparkplugNode(metrics, null);

        _nodeOptions = new SparkplugNodeOptions(
            brokerAddress: _config.transport["net"]["ip"],
            port: _config.transport["net"]["port"],
            userName: _config.transport["user"],
            clientId: new Guid().ToString(),
            password: _config.transport["password"],
            useTls: false,
            scadaHostIdentifier: "scada",
            groupIdentifier: "fanuc",
            edgeNodeIdentifier: Environment.MachineName,    // TODO: clean up hostname to spb spec
            reconnectInterval: TimeSpan.FromSeconds(30),
            webSocketParameters: null,
            proxyOptions: null,
            cancellationToken: new CancellationToken()
        );
        
        _node.OnDisconnected += () =>
        {
            Console.WriteLine("disconnected");
        };

        _node.NodeCommandReceived += metric =>
        {
            Console.WriteLine("node command incoming");
        };

        _node.StatusMessageReceived += s =>
        {
            Console.WriteLine($"status message received {s}");
        };

        await ConnectAsync();
        return null;
    }

    public override async Task ConnectAsync()
    {
        if (!_node.IsConnected)
        {
            try
            {
                await _node.Start(_nodeOptions);
                await _node.PublishMetrics(_nodeMetrics);
            }
            catch (Exception ex)
            {
                logger.Error(ex,$"[{machine.Id}] Broker connection error.");
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
                processIncoming(prefix, data);
                
                break;
                
            case "SWEEP_END":
                
                processIncoming("sweep", data);

                var changes = _current
                    .Except(_previous)
                    .ToDictionary();

                if (logger.IsTraceEnabled)
                {
                    logger.Trace($"[{machine.Id}] Total metric count: {_current.Count()}");
                    logger.Trace($"[{machine.Id}] Changed metric count: {changes.Count()}");
                    changes.ForEach(x =>
                    {
                        logger.Trace($"[{machine.Id}] Metric change: {x.Key} = {x.Value.ToString()}");
                    });
                }

                _previous = _current
                    .ToDictionary(
                        kvp => kvp.Key, 
                        kvp => kvp.Value);
                
                if (data.state.data.online == true && _deviceState == DeviceStateEnum.OFFLINE)
                {
                    await deviceBirthAsync(changes);
                    _deviceState = DeviceStateEnum.ONLINE;
                }
                else if (data.state.data.online == false && _deviceState == DeviceStateEnum.ONLINE)
                {
                    await deviceDeathAsync();
                    _deviceState = DeviceStateEnum.OFFLINE;
                }
                else
                {
                    await deviceUpdateAsync(changes);
                }
                
                break;
            
            case "INT_MODEL":

                break;
        }
    }

    void processIncoming(string prefix, dynamic data)
    {
        // flatten incoming data
        JObject jc = JObject.FromObject(data.state.data);
        var fc = jc.Flatten();
        
        // massage keys
        var dict = fc.Select(x=>
            {
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

    private async Task deviceBirthAsync(Dictionary<string, object> diff)
    {
        List<Metric> metrics = diff
            .Select(kv => makeMetric(kv.Key, kv.Value))
            .ToList();
        
        if(_node.IsConnected)
            await _node.PublishDeviceBirthMessage(metrics, machine.Id);
    }
    
    private async Task deviceDeathAsync()
    {
        if(_node.IsConnected)
            await _node.PublishDeviceDeathMessage(machine.Id);
    }
    
    private async Task deviceUpdateAsync(Dictionary<string, object> diff)
    {
        List<Metric> metrics = diff
            .Select(kv => makeMetric(kv.Key, kv.Value))
            .ToList();
        
        if(_node.IsConnected)
            await _node.PublishDeviceData(metrics, machine.Id);
    }

    private Metric makeMetric(string name, object value)
    {
        switch (Type.GetTypeCode(value.GetType()))
        {
            case TypeCode.Byte:
                return new Metric()
                {
                    Name = name, DataType = (uint)DataType.Int8, IntValue = Convert.ToUInt16(value)
                };
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Double:
                return new Metric()
                {
                    Name = name, DataType = (uint)DataType.Double, DoubleValue = Convert.ToDouble(value) 
                };
            case TypeCode.Boolean:
                return new Metric()
                {
                    Name = name, DataType = (uint)DataType.Boolean, BooleanValue = Convert.ToBoolean(value) 
                };
            case TypeCode.String:
                return new Metric()
                {
                    Name = name, DataType = (uint)DataType.String, StringValue = Convert.ToString(value) 
                };
            default:
                logger.Info($"[{machine.Id}] '{name}'({value.GetType().FullName}) converted to string metric.");
                
                return new Metric()
                {
                    Name = name, DataType = (uint)DataType.String, StringValue = Convert.ToString(value) 
                };
        }
    }
}
