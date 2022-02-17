using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using l99.driver.@base;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Exceptions;

namespace l99.driver.fanuc.transports
{
    public class MQTT : Transport
    {
        private IMqttClientOptions _options;
        private IMqttClient _client;
        private dynamic _config;

        private string _key = string.Empty;
        private int _connectionFailCount = 0;
        private bool _connectionSkipped = false;
        
        public MQTT(Machine machine, object cfg) : base(machine, cfg)
        {
            _config = cfg;
            
            _key = $"{_config.transport["net_type"]}://{_config.transport["net_ip"]}:{_config.transport["net_port"]}/{machine.Id}";
            
            IMqttFactory factory = new MqttFactory();
            MqttClientOptionsBuilder builder;

            switch (_config.transport["net_type"])
            {
                case "ws":
                    builder = new MqttClientOptionsBuilder()
                        .WithWebSocketServer($"{_config.transport["net_ip"]}:{_config.transport["net_port"]}");
                    break;
                default:
                    builder = new MqttClientOptionsBuilder()
                        .WithTcpServer(_config.transport["net_ip"], _config.transport["net_port"]);
                    break;
            }
            
            if (!_config.transport["anonymous"])
            {
                byte[] passwordBuffer = null;

                if (_config.transport["password"] != null)
                    passwordBuffer = Encoding.UTF8.GetBytes(_config.transport["password"]);

                builder = builder.WithCredentials(_config.transport["user"], passwordBuffer);
            }

            _options = builder.Build();
            _client = factory.CreateMqttClient();
        }

        public override async Task<dynamic?> CreateAsync()
        {
            await ConnectAsync();
            return null;
        }
        
        public override async Task SendAsync(params dynamic[] parameters)
        {
            string topic = parameters[0];
            string payload = parameters[1];
            bool retained = parameters[2];
            
            logger.Trace($"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()} PUB {payload.Length}b => {topic}\n{payload}");
            
            if (_client.IsConnected)
            {
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
            if (_config.transport["enabled"])
            {
                if (!_client.IsConnected)
                {
                    if (_connectionFailCount == 0)
                    {
                        logger.Info($"Connecting broker '{_key}': {_options.ChannelOptions}");
                    }
                    else
                    {
                        logger.Debug($"Connecting broker '{_key}': {_options.ChannelOptions}");
                    }
                    
                    try
                    {
                        await _client.ConnectAsync(_options, CancellationToken.None);
                        //_client.UseApplicationMessageReceivedHandler(async (e) => { await handleIncomingMessage(e); });
                        logger.Info($"Connected broker '{_key}': {_options.ChannelOptions}");
                        _connectionFailCount = 0;
                    }
                    catch (MqttCommunicationTimedOutException tex)
                    {
                        if (_connectionFailCount == 0)
                        {
                            logger.Warn($"Broker connection timeout '{_key}': {_options.ChannelOptions}");
                        }
                        else
                        {
                            logger.Debug($"Broker connection timeout '{_key}': {_options.ChannelOptions}");
                        }

                        _connectionFailCount++;
                    }
                    catch (MqttCommunicationException ex)
                    {
                        if (_connectionFailCount == 0)
                        {
                            logger.Warn($"Broker connection failed '{_key}': {_options.ChannelOptions}");
                        }
                        else
                        {
                            logger.Debug($"Broker connection failed '{_key}': {_options.ChannelOptions}");
                        }
                        
                        _connectionFailCount++;
                    }
                }
            }
            else
            {
                if (!_connectionSkipped)
                {
                    logger.Info($"Skipping broker connection '{_key}': {_options.ChannelOptions}");
                    _connectionSkipped = true;
                }
            }
        }
    }
}