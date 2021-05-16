using System;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace fanuc.mqtt
{
    public class Broker
    {
        private dynamic _options;
            
        private IMqttClient _client;

        public IMqttClient Client
        {
            get => _client;
        }

        private bool MQTT_CONNECT = false;
        private bool MQTT_PUBLISH_STATUS = false;
        private bool MQTT_PUBLISH_ARRIVALS = false;
        private bool MQTT_PUBLISH_CHANGES = false;

        public Broker(dynamic cfg)
        {
            MQTT_CONNECT = cfg.enabled;
            MQTT_PUBLISH_STATUS = cfg.pub_status;
            MQTT_PUBLISH_ARRIVALS = cfg.pub_arrivals;
            MQTT_PUBLISH_CHANGES = cfg.pub_changes;
            
            var factory = new MqttFactory();
            _options = new MqttClientOptionsBuilder()
                .WithTcpServer(cfg.ip, cfg.port)
                .Build();
            _client = factory.CreateMqttClient();
        }

        public void Connect()
        {
            if (MQTT_CONNECT)
            {
                var r = _client.ConnectAsync(_options, CancellationToken.None).Result;
            }
        }

        public void PublishArrivalStatus(string topic, string payload, bool retained = true)
        {
            if (MQTT_CONNECT && MQTT_PUBLISH_STATUS)
            {
                PublishArrival(topic, payload, retained);
            }
        }

        public void PublishArrival(string topic, string payload, bool retained = true)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()} ARRIVE {payload.Length}b => {topic}");

            if (MQTT_CONNECT && MQTT_PUBLISH_ARRIVALS)
            {
                var msg = new MqttApplicationMessageBuilder()
                    .WithRetainFlag(retained)
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .Build();
                
                var r = _client.PublishAsync(msg, CancellationToken.None);
            }
        }
        
        public void PublishChangeStatus(string topic, string payload, bool retained = true)
        {
            if (MQTT_CONNECT && MQTT_PUBLISH_STATUS)
            {
                PublishChange(topic, payload, retained);
            }
        }
        
        public void PublishChange(string topic, string payload, bool retained = true)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()} CHANGE {payload.Length}b => {topic}");
            
            if (MQTT_CONNECT && MQTT_PUBLISH_CHANGES)
            {
                var msg = new MqttApplicationMessageBuilder()
                    .WithRetainFlag(retained)
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .Build();
                
                var r = _client.PublishAsync(msg, CancellationToken.None);
            }
        }
    }
}