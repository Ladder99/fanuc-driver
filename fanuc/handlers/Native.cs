using System;
using System.Threading;
using fanuc.veneers;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json.Linq;

namespace fanuc.handlers
{
    public class Native: Handler
    {
        private bool MQTT_CONNECT = false;
        private bool MQTT_PUBLISH_STATUS = false;
        private bool MQTT_PUBLISH_ARRIVALS = false;
        private bool MQTT_PUBLISH_CHANGES = false;

        private IMqttClient _mqtt;
        
        public Native(Machine machine) : base(machine)
        {
            
        }

        public override void Initialize(dynamic config)
        {
            MQTT_CONNECT = config.mqtt_enabled;
            MQTT_PUBLISH_STATUS = config.mqtt_pub_status;
            MQTT_PUBLISH_ARRIVALS = config.mqtt_pub_arrivals;
            MQTT_PUBLISH_CHANGES = config.mqtt_pub_changes;
            
            var factory = new MqttFactory();
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(config.mqtt_ip, config.mqtt_port)
                .Build();
            _mqtt = factory.CreateMqttClient();
            if (MQTT_CONNECT)
            {
                var r = _mqtt.ConnectAsync(options, CancellationToken.None).Result;
            }
        }
        
        protected override dynamic? beforeDataArrival(Veneers veneers, Veneer veneer)
        {
            return null;
        }
        
        public override dynamic? OnDataArrival(Veneers veneers, Veneer veneer, dynamic? beforeArrival)
        {
            dynamic payload = new
            {
                observation = new
                {
                    time =  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                    machine = veneers.Machine.Id,
                    name = veneer.Name,
                    marker = veneer.Marker
                },
                source = new
                {
                    method = veneer.IsInternal ? "" : veneer.LastArrivedInput.method,
                    invocationMs = veneer.IsInternal ? -1 : veneer.LastArrivedInput.invocationMs,
                    data = veneer.IsInternal ? new { } : veneer.LastArrivedInput.request.GetType().GetProperty(veneer.LastArrivedInput.method).GetValue(veneer.LastArrivedInput.request, null)
                },
                delta = new
                {
                    time = veneer.ArrivalDelta,
                    data = veneer.LastArrivedValue
                }
            };

            return payload;
        }
        
        protected override void afterDataArrival(Veneers veneers, Veneer veneer, dynamic? onArrival)
        {
            var topic = $"fanuc/{veneers.Machine.Id}-all/{veneer.Name}{(veneer.SliceKey == null ? string.Empty : "/" + veneer.SliceKey.ToString())}";
            string payload_string = JObject.FromObject(onArrival).ToString();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()} ARRIVE {payload_string.Length}b => {topic}");
                
            if (MQTT_CONNECT && MQTT_PUBLISH_ARRIVALS)
            {
                var msg = new MqttApplicationMessageBuilder()
                    .WithRetainFlag(true)
                    .WithTopic(topic)
                    .WithPayload(payload_string)
                    .Build();
                
                var r = _mqtt.PublishAsync(msg, CancellationToken.None);
            }
        }
        
        protected override dynamic? beforeDataChange(Veneers veneers, Veneer veneer)
        {
            return null;
        }
        
        public override dynamic? OnDataChange(Veneers veneers, Veneer veneer, dynamic? beforeChange)
        {
            dynamic payload = new
            {
                observation = new
                {
                    time =  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                    machine = veneers.Machine.Id,
                    name = veneer.Name,
                    marker = veneer.Marker
                },
                source = new
                {
                    method = veneer.LastChangedInput.method,
                    veneer.LastChangedInput.invocationMs,
                    data = veneer.LastChangedInput.request.GetType().GetProperty(veneer.LastChangedInput.method).GetValue(veneer.LastChangedInput.request, null)
                },
                delta = new
                {
                    time = veneer.ChangeDelta,
                    data = veneer.LastChangedValue
                }
            };

            return payload;
        }
        
        protected override void afterDataChange(Veneers veneers, Veneer veneer, dynamic? onChange)
        {
            var topic = $"fanuc/{veneers.Machine.Id}/{veneer.Name}{(veneer.SliceKey == null ? string.Empty : "/" + veneer.SliceKey.ToString())}";
            string payload_string = JObject.FromObject(onChange).ToString();
                
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()} CHANGE {payload_string.Length}b => {topic}");

            if (MQTT_CONNECT && MQTT_PUBLISH_CHANGES)
            {
                var msg = new MqttApplicationMessageBuilder()
                    .WithRetainFlag(true)
                    .WithTopic(topic)
                    .WithPayload(payload_string)
                    .Build();
                
                var r = _mqtt.PublishAsync(msg, CancellationToken.None);
            }
        }
        
        protected override dynamic? beforeDataError(Veneers veneers, Veneer veneer)
        {
            return null;
        }
        
        public override dynamic? OnError(Veneers veneers, Veneer veneer, dynamic? beforeError)
        {
            return null;
        }
        
        protected override void afterDataError(Veneers veneers, Veneer veneer, dynamic? onError)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(new
            {
                method = veneer.LastArrivedInput.method, rc = veneer.LastArrivedInput.rc
            });
        }
        
        protected override dynamic? beforeSweepComplete(Machine machine)
        {
            return null;
        }
        
        public override dynamic? OnCollectorSweepComplete(Machine machine, dynamic? beforeSweepComplete)
        {
            dynamic payload = new
            {
                observation = new
                {
                    time =  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                    machine = machine.Id,
                    name = "PING"
                },
                source = new
                {
                    data = machine.Info
                },
                delta = new
                {
                    data = machine.CollectorSuccess ? "OK" : "NOK"
                }
            };

            return payload;
        }
        
        protected override void afterSweepComplete(Machine machine, dynamic? onSweepComplete)
        {
            if (MQTT_CONNECT && MQTT_PUBLISH_STATUS)
            {
                if (MQTT_PUBLISH_CHANGES)
                {
                    var msg = new MqttApplicationMessageBuilder()
                        .WithTopic($"fanuc/{machine.Id}/PING")
                        .WithPayload(JObject.FromObject(onSweepComplete).ToString())
                        .WithRetainFlag()
                        .Build();

                    var r = _mqtt.PublishAsync(msg, CancellationToken.None).Result;
                }

                if (MQTT_PUBLISH_ARRIVALS)
                {
                    var msg = new MqttApplicationMessageBuilder()
                        .WithTopic($"fanuc/{machine.Id}-all/PING")
                        .WithPayload(JObject.FromObject(onSweepComplete).ToString())
                        .WithRetainFlag()
                        .Build();

                    var r = _mqtt.PublishAsync(msg, CancellationToken.None).Result;
                }
            }
        }
    }
}