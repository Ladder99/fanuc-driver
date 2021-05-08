using System;
using System.Collections.Generic;
using System.Threading;
using fanuc.veneers;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json.Linq;

namespace fanuc
{
    class Program
    {
        private static IMqttClient _mqtt;
        private static fanuc.Machines _machines = new fanuc.Machines();
        
        static void Main(string[] args)
        {
            setupMqtt("10.20.30.102");  // "172.16.10.3"
            createMachines(new List<dynamic>()
            {
                new { enabled = false, id = "naka", ip = "172.16.13.100", port = 8193, timeout = 2, collector = typeof(collectors.Basic)},
                new { enabled = true, id = "sim", ip = "10.20.30.101", port = 8193, timeout = 2, collector = typeof(collectors.Basic)}
            });
            
            createVeneers();
            processVeneers();
        }

        static void setupMqtt(string ip)
        {
            var factory = new MqttFactory();
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(ip)
                .Build();
            _mqtt = factory.CreateMqttClient();
            var r = _mqtt.ConnectAsync(options).Result;
        }

        static void createMachines(List<dynamic> machineConfigs)
        {
            Action<Veneers, Veneer> on_data_change = (vv, v) =>
            {
                dynamic payload = new
                {
                    observation = new
                    {
                        time =  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds(),
                        machine = vv.Machine.Id,
                        name = v.Name,
                        marker = v.Marker
                    },
                    source = new
                    {
                        method = v.LastInput.method,
                        data = v.LastInput.request.GetType().GetProperty(v.LastInput.method).GetValue(v.LastInput.request, null)
                    },
                    delta = new
                    {
                        time = v.ChangeDelta,
                        data = v.DataDelta
                    }
                };

                var topic = $"fanuc/{vv.Machine.Id}/{v.Name}{(v.SliceKey == null ? string.Empty : "/" + v.SliceKey.ToString())}";
                var payload_string = JObject.FromObject(payload).ToString();
                
                Console.WriteLine(topic);
                Console.WriteLine(payload_string);
                Console.WriteLine();
                
                var msg = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload_string)
                    .WithRetainFlag()
                    .Build();
                var r = _mqtt.PublishAsync(msg, CancellationToken.None);
            };

            Action<Veneers, Veneer> on_error = (vv, v) =>
            {
                //Console.WriteLine(new { method = v.LastInput.method, rc = v.LastInput.rc });
            };

            foreach (var cfg in machineConfigs)
            {
                var machine = _machines.Add(cfg.enabled, cfg.id, cfg.ip, (ushort)cfg.port, (short)cfg.timeout);
                machine.AddCollector(cfg.collector);
                machine.Veneers.OnDataChange = on_data_change;
                machine.Veneers.OnError = on_error;
            }
        }
        
        static void createVeneers()
        {
            foreach (var machine in _machines[null])
            {
                machine.InitCollector();
            }
        }
        
        static void processVeneers()
        {
            while (true)
            {
                foreach (var machine in _machines[null])
                {
                    machine.RunCollector();
                    
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
                        
                    var msg = new MqttApplicationMessageBuilder()
                        .WithTopic($"fanuc/{machine.Id}/PING")
                        .WithPayload(JObject.FromObject(payload).ToString())
                        .WithRetainFlag()
                        .Build();
                    var r = _mqtt.PublishAsync(msg, CancellationToken.None).Result;
                }
                
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}      