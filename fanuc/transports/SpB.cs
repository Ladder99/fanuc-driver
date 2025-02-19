using l99.driver.@base;
using l99.driver.fanuc.utils;
using SparkplugNet.Core.Node;
using SparkplugNet.VersionB;
using SparkplugNet.VersionB.Data;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.transports;

// ReSharper disable once UnusedType.Global
public class SpB : Transport
{
    private Dictionary<string, dynamic> _current = new();

    private DeviceStateEnum _deviceState = DeviceStateEnum.Offline;

    private SparkplugNode _node = null!;
    private List<Metric> _nodeMetrics = null!;
    private SparkplugNodeOptions _nodeOptions = null!;

    private Dictionary<string, dynamic> _previous = new();

    public SpB(Machine machine) : base(machine)
    {
        if (!Machine.Configuration.transport.ContainsKey("anonymous"))
            Machine.Configuration.transport.Add("anonymous", true);
        
        if (!Machine.Configuration.transport.ContainsKey("user"))
            Machine.Configuration.transport.Add("user", string.Empty);
        
        if (!Machine.Configuration.transport.ContainsKey("password"))
            Machine.Configuration.transport.Add("password", string.Empty);
        
        if (!Machine.Configuration.transport.ContainsKey("reconnect_s"))
            Machine.Configuration.transport.Add("reconnect_s", 30);
        
        if (!Machine.Configuration.transport.ContainsKey("net"))
        {
            Machine.Configuration.transport.Add("net", new Dictionary<object, object>()
            {
                { "type", "tcp" },
                { "ip", "127.0.0.1" },
                { "port", 1883 }
            });
        }
        else if (Machine.Configuration.transport["net"] == null)
        {
            Machine.Configuration.transport["net"] = new Dictionary<object, object>()
            {
                { "type", "tcp" },
                { "ip", "127.0.0.1" },
                { "port", 1883 }
            };
        }
        
        if (!Machine.Configuration.transport.ContainsKey("overrides"))
        {
            Machine.Configuration.transport.Add("overrides", new Dictionary<object, object>()
            {
                { "scada_id", null! },
                { "group_id", null! },
                { "node_id", null! },
                { "device_id", null! }
            });
        }
        else if (Machine.Configuration.transport["overrides"] == null)
        {
            Machine.Configuration.transport["overrides"] = new Dictionary<object, object>()
            {
                { "scada_id", null! },
                { "group_id", null! },
                { "node_id", null! },
                { "device_id", null! }
            };
        }
    }

    public override async Task<dynamic?> CreateAsync()
    {
        _nodeMetrics = new List<Metric>
        {
            new()
            {
                Name = "IpAddress", DataType = DataType.String,
                StringValue = string.Join(';', Network.GetAllLocalIPv4())
            }
        };

        // ReSharper disable once RedundantArgumentDefaultValue
        _node = new SparkplugNode(_nodeMetrics);

        var scadaId = Machine.Configuration.transport["overrides"]["scada_id"] == null ? "scada" : Machine.Configuration.transport["overrides"]["scada_id"];
        var clientId = Machine.Configuration.transport["overrides"]["device_id"] == null ? $"fanuc_{Machine.Id}" : Machine.Configuration.transport["overrides"]["device_id"];
        var groupId = Machine.Configuration.transport["overrides"]["group_id"] == null ? "fanuc" : Machine.Configuration.transport["overrides"]["group_id"];
        var nodeId = Machine.Configuration.transport["overrides"]["node_id"] == null ? $"{Environment.MachineName}" : Machine.Configuration.transport["overrides"]["node_id"];

        // ReSharper disable once UseObjectOrCollectionInitializer
        _nodeOptions = new SparkplugNodeOptions(
            brokerAddress: Machine.Configuration.transport["net"]["ip"],
            port: Machine.Configuration.transport["net"]["port"],
            userName: Machine.Configuration.transport["user"],
            clientId: clientId,
            password: Machine.Configuration.transport["password"],
            useTls: false,
            scadaHostIdentifier: scadaId,
            groupIdentifier: groupId,
            edgeNodeIdentifier: nodeId,
            reconnectInterval: TimeSpan.FromSeconds(Machine.Configuration.transport["reconnect_s"]),
            webSocketParameters: null,
            proxyOptions: null,
            cancellationToken: new CancellationToken()
        );
        
        _nodeOptions.AddSessionNumberToDataMessages = true;

        // ReSharper disable once UnusedParameter.Local
        _node.DisconnectedAsync += async args =>
        {
            Logger.Warn($"[{Machine.Id}] SpB node disconnected.");
            await Task.FromResult(0);
        };

        // ReSharper disable once UnusedParameter.Local
        _node.NodeCommandReceivedAsync += async args =>
        {
            Logger.Info($"[{Machine.Id}] SpB node incoming command.");
            await Task.FromResult(0);
        };

        _node.StatusMessageReceivedAsync += async args =>
        {
            Logger.Warn($"[{Machine.Id}] SpB node status message '{args.Status}.");
            await Task.FromResult(0);
        };
        
        await ConnectAsync();
        return null;
    }

    public override async Task ConnectAsync()
    {
        if (Machine.Enabled)
            if (!_node.IsConnected)
                try
                {
                    await _node.Start(_nodeOptions);
                    await _node.PublishMetrics(_nodeMetrics);
                    Logger.Info($"[{Machine.Id}] SpB node connected.");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"[{Machine.Id}] Broker connection error.");
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

                var prefix = $"{veneer.Name}{(veneer.SliceKey == null ? null : "/" + veneer.SliceKey)}";
                ProcessIncoming(prefix, data);

                break;

            case "SWEEP_END":

                ProcessIncoming("sweep", data);

                var changesTemp = _current.Except(_previous);
                var changes = System.Linq.Enumerable.ToDictionary(changesTemp);

                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace($"[{Machine.Id}] Total metric count: {_current.Count()}");
                    Logger.Trace($"[{Machine.Id}] Changed metric count: {changes.Count()}");
                    changes.ForEach(x =>
                    {
                        Logger.Trace($"[{Machine.Id}] Metric change: {x.Key} = {x.Value.ToString()}");
                    });
                }

                _previous = _current
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value);

                if (data.state.data.online == true && _deviceState == DeviceStateEnum.Offline)
                {
                    Logger.Info($"[{Machine.Id}] SpB device birth.");
                    var success = await DeviceBirthAsync(_current);
                    _deviceState = success ? DeviceStateEnum.Online : DeviceStateEnum.MustRebirth;
                }
                else if (data.state.data.online == false && _deviceState == DeviceStateEnum.Online)
                {
                    Logger.Info($"[{Machine.Id}] SpB device death.");
                    await DeviceDeathAsync();
                    _deviceState = DeviceStateEnum.Offline;
                }
                else if (data.state.data.online == true && _deviceState == DeviceStateEnum.MustRebirth)
                {
                    Logger.Info($"[{Machine.Id}] SpB device re-birth.");
                    var success = await DeviceBirthAsync(_current);
                    if (success) _deviceState = DeviceStateEnum.Online;
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

    private void ProcessIncoming(string prefix, dynamic data)
    {
        // flatten incoming data
        JObject jc = JObject.FromObject(data.state.data);
        var fc = jc.Flatten();

        // massage keys
        var dict = fc.Select(x =>
            {
                // ReSharper disable once ConvertToLambdaExpression
                return new KeyValuePair<string, object>(
                    //$"{prefix.Replace('/','.')}.{x.Key.Replace('[','_').Replace("]", string.Empty)}",
                    //$"{prefix.Replace('/','.')}.{x.Key}",
                    $"{prefix}/{x.Key.Replace('.', '/')}",
                    x.Value);
            })
            .ToDictionary(
                x => x.Key,
                x => x.Value);

        // remove arrays for now, we need to deal with those differently
        var keys = dict.Keys.Where(k => k.Contains('[')).ToList();
        foreach (var key in keys) dict.Remove(key);

        // update current with incoming changes
        _current = dict
            .Concat(_current)
            .GroupBy(e => e.Key)
            .ToDictionary(g => g.Key, g => g.First().Value);
    }

    private async Task<bool> DeviceBirthAsync(Dictionary<string, object> diff)
    {
        var metrics = diff
            .Select(kv => MakeMetric(kv.Key, kv.Value))
            .ToList();

        if (_node.IsConnected)
        {
            try
            {
                await _node.PublishDeviceBirthMessage(metrics, 
                    Machine.Configuration.transport["overrides"]["device_id"] == null ? Machine.Id : Machine.Configuration.transport["overrides"]["device_id"]);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"[{Machine.Id}] SpB device birth error.");
                return false;
            }

            return true;
        }

        return false;
    }

    private async Task DeviceDeathAsync()
    {
        if (_node.IsConnected)
            try
            {
                await _node.PublishDeviceDeathMessage(Machine.Id);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"[{Machine.Id}] SpB device death error.");
            }
    }

    private async Task DeviceUpdateAsync(Dictionary<string, object> diff)
    {
        var metrics = diff
            .Select(kv => MakeMetric(kv.Key, kv.Value))
            .ToList();

        if (_node.IsConnected)
            try
            {
                await _node.PublishDeviceData(metrics, Machine.Id);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"[{Machine.Id}] SpB device data error.");
            }
    }

    private Metric MakeMetric(string name, object value)
    {
        switch (Type.GetTypeCode(value.GetType()))
        {
            case TypeCode.Byte:
                return new Metric
                {
                    Name = name, DataType = DataType.Int8, IntValue = Convert.ToUInt16(value)
                };
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Double:
                return new Metric
                {
                    Name = name, DataType = DataType.Double, DoubleValue = Convert.ToDouble(value)
                };
            case TypeCode.Boolean:
                return new Metric
                {
                    Name = name, DataType = DataType.Boolean, BooleanValue = Convert.ToBoolean(value)
                };
            case TypeCode.String:
                return new Metric
                {
                    Name = name, DataType = DataType.String, StringValue = Convert.ToString(value) ?? string.Empty
                };
            default:
                Logger.Info($"[{Machine.Id}] '{name}'({value.GetType().FullName}) converted to string metric.");

                return new Metric
                {
                    Name = name, DataType = DataType.String, StringValue = Convert.ToString(value) ?? string.Empty
                };
        }
    }

    private enum DeviceStateEnum
    {
        Offline,
        Online,
        MustRebirth
    }
}