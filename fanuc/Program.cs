using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using fanuc.veneers;
using MQTTnet;
using MQTTnet.Client.Options;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace fanuc
{
    class Program
    {
        static void Main(string[] args)
        {
            var config_file = "config.yml";
            dynamic config = readConfig(config_file);
            dynamic mqtt = setupMqtt(config);
            dynamic machines = createMachines(config, mqtt);
            createVeneers(machines);
            processVeneers(machines, mqtt);
        }

        static dynamic readConfig(string config_file)
        {
            var input = new StreamReader(config_file);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return deserializer.Deserialize(input);
        }
        
        static dynamic setupMqtt(dynamic config)
        {
            var factory = new MqttFactory();
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(config["broker"]["net_ip"], config["broker"]["net_port"])
                .Build();
            var client = factory.CreateMqttClient();
            var r = client.ConnectAsync(options, CancellationToken.None).Result;
            return client;
        }

        static dynamic createMachines(dynamic config, dynamic mqtt)
        {
            var machine_confs = new List<dynamic>();

            foreach (dynamic machine_conf in config["machines"])
            {
                machine_confs.Add(new
                {
                    enabled = machine_conf["enabled"],
                    id = machine_conf["id"],
                    ip = machine_conf["net_ip"],
                    port = machine_conf["net_port"],
                    timeout = machine_conf["net_timeout_s"],
                    collector = machine_conf["strategy_type"],
                    collectorSweepMs = machine_conf["sweep_ms"]
                });
            }
            
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
                string payload_string = JObject.FromObject(payload).ToString();
                
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(topic);
                Console.WriteLine(payload_string);
                Console.WriteLine();
                
                var msg = new MqttApplicationMessageBuilder()
                    .WithRetainFlag(true)
                    .WithTopic(topic)
                    .WithPayload(payload_string)
                    .Build();
                var r = mqtt.PublishAsync(msg, CancellationToken.None);
            };

            Action<Veneers, Veneer> on_error = (vv, v) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(new { method = v.LastInput.method, rc = v.LastInput.rc });
            };

            dynamic machines = new Machines();
            
            foreach (var cfg in machine_confs)
            {
                var machine = machines.Add(cfg.enabled, cfg.id, cfg.ip, (ushort)cfg.port, (short)cfg.timeout);
                machine.AddCollector(Type.GetType(cfg.collector), cfg.collectorSweepMs);
                machine.Veneers.OnDataChange = on_data_change;
                machine.Veneers.OnError = on_error;
            }

            return machines;
        }
        
        static void createVeneers(dynamic machines)
        {
            foreach (var machine in machines[null])
            {
                machine.InitCollector();
            }
        }
        
        static void processVeneers(dynamic machines, dynamic mqtt)
        {
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                
                foreach (var machine in machines[null])
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
                    var r = mqtt.PublishAsync(msg, CancellationToken.None).Result;
                }
            }
        }
    }
}      