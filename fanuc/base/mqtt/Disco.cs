using System;
using System.Collections.Generic;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json.Linq;

namespace l99.driver.@base.mqtt
{
    public class Disco
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

        public Disco()
        {
            
        }

        public void Add(string machineId, Broker broker)
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
            string payload = JObject.FromObject(_items).ToString();
            broker.Publish(topic, payload);
        }
    }
}