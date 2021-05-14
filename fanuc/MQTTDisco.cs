using System;
using System.Collections.Generic;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json.Linq;

namespace fanuc.utils
{
    public class MQTTDisco
    {
        private class MQTTDiscoItem
        {
            public long added;
            public long seen;
            public string machineId;
            public string arrivalTopic;
            public string changeTopic;
        }
        
        private Dictionary<string, MQTTDiscoItem> _items = new Dictionary<string, MQTTDiscoItem>();

        private IMqttClient _mqtt;
        private dynamic _mqttConfig;
        
        public MQTTDisco(dynamic mqtt, dynamic mqttConfig)
        {
            _mqtt = mqtt;
            _mqttConfig = mqttConfig;
        }

        public void Add(string machineId)
        {
            if (!_items.ContainsKey(machineId))
            {
                var epoch = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

                _items.Add(machineId, new MQTTDiscoItem()
                {
                    machineId = machineId,
                    added = epoch,
                    seen = epoch,
                    arrivalTopic = $"fanuc/{machineId}-all",
                    changeTopic = $"fanuc/{machineId}"
                });
            }
            else
            {
                _items[machineId].seen = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            }

            string topic = "fanuc/DISCO";
            string payload_string = JObject.FromObject(_items).ToString();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()} DISCO {payload_string.Length}b => {topic}");
        
            if (_mqttConfig["enabled"] && _mqttConfig["publish_status"])
            {
                var msg = new MqttApplicationMessageBuilder()
                    .WithRetainFlag(true)
                    .WithTopic(topic)
                    .WithPayload(payload_string)
                    .Build();
            
                var r = _mqtt.PublishAsync(msg, CancellationToken.None).Result;
            }
        }
    }
}