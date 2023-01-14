using l99.driver.@base;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using Scriban;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.transports;

// ReSharper disable once InconsistentNaming
public class MQTT : Transport
{
    private MqttClientOptions _options;
    private IMqttClient _client;
    private readonly dynamic _config;

    private string _key = string.Empty;
    private int _connectionFailCount;
    private bool _connectionSkipped;
    
    private Template _topicTemplate;
    
#pragma warning disable CS8618
    public MQTT(Machine machine, object cfg) : base(machine, cfg)
#pragma warning restore CS8618
    {
        _config = cfg;
    }

    public override async Task<dynamic?> CreateAsync()
    {
        //TODO: validate config
        _topicTemplate = Template.Parse(_config.transport["topic"]);
        _key = $"{_config.transport["net"]["type"]}://{_config.transport["net"]["ip"]}:{_config.transport["net"]["port"]}/{machine.Id}";
        
        var factory = new MqttFactory();
        MqttClientOptionsBuilder builder;

        switch (_config.transport["net"]["type"])
        {
            case "ws":
                builder = new MqttClientOptionsBuilder()
                    .WithWebSocketServer($"{_config.transport["net"]["ip"]}:{_config.transport["net"]["port"]}");
                break;
            default:
                builder = new MqttClientOptionsBuilder()
                    .WithTcpServer(_config.transport["net"]["ip"], _config.transport["net"]["port"]);
                break;
        }
        
        if (!_config.transport["anonymous"])
        {
            byte[]? passwordBuffer = null;

            if (_config.transport["password"] != null)
                passwordBuffer = Encoding.UTF8.GetBytes(_config.transport["password"]);

            builder = builder.WithCredentials(_config.transport["user"], passwordBuffer);
        }

        _options = builder.Build();
        _client = factory.CreateMqttClient();
        
        await ConnectAsync();
        return null;
    }
    
    public override async Task SendAsync(params dynamic[] parameters)
    {
        var @event = parameters[0];
        var veneer = parameters[1];
        var data = parameters[2];

        string topic;
        string payload;
        bool retained = true;
        
        switch (@event)
        {
            case "DATA_ARRIVE":
                topic = await _topicTemplate.RenderAsync(new { machine, veneer}, member => member.Name);
                payload = JObject.FromObject(data).ToString(Formatting.None);
                break;
            
            case "SWEEP_END":
                topic = $"fanuc/{machine.Id}/sweep";
                payload = JObject.FromObject(data).ToString(Formatting.None);

                await ConnectAsync();
                
                break;
            
            case "INT_MODEL":
                topic = $"fanuc/{machine.Id}/$model";
                payload = data;
                break;
            
            default:
                return;
        }
        
        if (_client.IsConnected)
        {
            Logger.Trace($"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()} PUB {payload.Length}b => {topic}\n{payload}");
            
            var msg = new MqttApplicationMessageBuilder()
                .WithRetainFlag(retained)
                .WithTopic(topic)
                .WithPayload(payload)
                .Build();
            
            await _client.PublishAsync(msg, CancellationToken.None);
        }
    }
    
    public override async Task ConnectAsync()
    {
        if (_config.machine.enabled)
        {
            if (!_client.IsConnected)
            {
                if (_connectionFailCount == 0)
                {
                    Logger.Info($"[{machine.Id}] Connecting broker '{_key}': {_options.ChannelOptions}");
                }
                else
                {
                    Logger.Debug($"[{machine.Id}] Connecting broker '{_key}': {_options.ChannelOptions}");
                }
                
                try
                {
                    await _client.ConnectAsync(_options, CancellationToken.None);
                    //_client.UseApplicationMessageReceivedHandler(async (e) => { await handleIncomingMessage(e); });
                    Logger.Info($"[{machine.Id}] Connected broker '{_key}': {_options.ChannelOptions}");
                    _connectionFailCount = 0;
                }
                catch (MqttCommunicationTimedOutException)
                {
                    if (_connectionFailCount == 0)
                    {
                        Logger.Warn($"[{machine.Id}] Broker connection timeout '{_key}': {_options.ChannelOptions}");
                    }
                    else
                    {
                        Logger.Debug($"[{machine.Id}] Broker connection timeout '{_key}': {_options.ChannelOptions}");
                    }

                    _connectionFailCount++;
                }
                catch (MqttCommunicationException)
                {
                    if (_connectionFailCount == 0)
                    {
                        Logger.Warn($"[{machine.Id}] Broker connection failed '{_key}': {_options.ChannelOptions}");
                    }
                    else
                    {
                        Logger.Debug($"[{machine.Id}] Broker connection failed '{_key}': {_options.ChannelOptions}");
                    }
                    
                    _connectionFailCount++;
                }
            }
        }
        else
        {
            if (!_connectionSkipped)
            {
                Logger.Info($"[{machine.Id}] Skipping broker connection '{_key}': {_options.ChannelOptions}");
                _connectionSkipped = true;
            }
        }
    }
    
    public override async Task OnGenerateIntermediateModelAsync(dynamic model)
    {
        await SendAsync("INT_MODEL", null, model.model);
    }
}