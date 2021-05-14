using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using fanuc.utils;
using fanuc.veneers;
using MQTTnet;
using MQTTnet.Client.Options;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace fanuc
{
    partial class Program
    {
        private static bool MQTT_CONNECT = false;
        private static bool MQTT_PUBLISH_STATUS = false;
        private static bool MQTT_PUBLISH_ARRIVALS = false;
        private static bool MQTT_PUBLISH_CHANGES = false;

        static void Main(string[] args)
        {
            string config_file = getArgument(args, "--config", "config.yml");
            dynamic config = readConfig(config_file);
            dynamic mqtt = setupMqtt(config);
            dynamic machines = createMachines(config, mqtt);
            createVeneers(machines);
            processVeneers(machines, mqtt);
        }

        static string getArgument(string[] args, string option, string defaultValue)
        {
            var value = args.SkipWhile(i => i != option).Skip(1).Take(1).FirstOrDefault();
            return string.IsNullOrEmpty(value) ? defaultValue : value;
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
            MQTT_CONNECT = config["broker"]["enabled"];
            MQTT_PUBLISH_STATUS = config["broker"]["publish_status"];
            MQTT_PUBLISH_ARRIVALS = config["broker"]["publish_arrivals"];
            MQTT_PUBLISH_CHANGES = config["broker"]["publish_changes"];
            
            var factory = new MqttFactory();
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(config["broker"]["net_ip"], config["broker"]["net_port"])
                .Build();
            var client = factory.CreateMqttClient();
            if (MQTT_CONNECT)
            {
                var r = client.ConnectAsync(options, CancellationToken.None).Result;
            }

            return client;
        }

        static dynamic createMachines(dynamic config, dynamic mqtt)
        {
            var mqtt_disco = new MQTTDisco(mqtt, config["broker"]);
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
                    collectorSweepMs = machine_conf["sweep_ms"],
                    handler = machine_conf["handler_type"]
                });
            }

            Machines machines = new Machines();
            
            foreach (var cfg in machine_confs)
            {
                Machine machine = machines.Add(cfg.enabled, cfg.id, cfg.ip, (ushort)cfg.port, (short)cfg.timeout);
                machine.AddCollector(Type.GetType(cfg.collector), cfg.collectorSweepMs);
                machine.AddHandler(Type.GetType(cfg.handler),
                    (Func<Veneers,Veneer,dynamic?>)((vv,v) =>
                    {
                        mqtt_disco.Add(vv.Machine.Id);
                        return null;
                    }),
                    (Action<Veneers,Veneer,dynamic?>)((vv,v,x) =>
                    {
                        var topic = $"fanuc/{vv.Machine.Id}-all/{v.Name}{(v.SliceKey == null ? string.Empty : "/" + v.SliceKey.ToString())}";
                        string payload_string = JObject.FromObject(x).ToString();

                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine($"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()} ARRIVE {payload_string.Length}b => {topic}");
                
                        if (MQTT_CONNECT && MQTT_PUBLISH_ARRIVALS)
                        {
                            var msg = new MqttApplicationMessageBuilder()
                                .WithRetainFlag(true)
                                .WithTopic(topic)
                                .WithPayload(payload_string)
                                .Build();
                
                            var r = mqtt.PublishAsync(msg, CancellationToken.None);
                        }
                    }),
                    (Func<Veneers,Veneer,dynamic?>)((vv,v) =>
                    {
                        mqtt_disco.Add(vv.Machine.Id);
                        return null;
                    }),
                    (Action<Veneers,Veneer,dynamic?>)((vv,v,x) =>
                    {
                        var topic = $"fanuc/{vv.Machine.Id}/{v.Name}{(v.SliceKey == null ? string.Empty : "/" + v.SliceKey.ToString())}";
                        string payload_string = JObject.FromObject(x).ToString();
                
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds()} CHANGE {payload_string.Length}b => {topic}");

                        if (MQTT_CONNECT && MQTT_PUBLISH_CHANGES)
                        {
                            var msg = new MqttApplicationMessageBuilder()
                                .WithRetainFlag(true)
                                .WithTopic(topic)
                                .WithPayload(payload_string)
                                .Build();
                
                            var r = mqtt.PublishAsync(msg, CancellationToken.None);
                        }
                    }),
                    (Func<Veneers,Veneer,dynamic?>)((vv,v) =>
                    {
                        return null;
                    }),
                    (Action<Veneers,Veneer,dynamic?>)((vv,v,x) =>
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(new
                        {
                            method = v.LastArrivedInput.method, rc = v.LastArrivedInput.rc
                        });
                    }));
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
                Thread.Sleep(1000);
                
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
                       
                    if (MQTT_CONNECT && MQTT_PUBLISH_STATUS)
                    {
                        if (MQTT_PUBLISH_CHANGES)
                        {
                            var msg = new MqttApplicationMessageBuilder()
                                .WithTopic($"fanuc/{machine.Id}/PING")
                                .WithPayload(JObject.FromObject(payload).ToString())
                                .WithRetainFlag()
                                .Build();

                            var r = mqtt.PublishAsync(msg, CancellationToken.None).Result;
                        }

                        if (MQTT_PUBLISH_ARRIVALS)
                        {
                            var msg = new MqttApplicationMessageBuilder()
                                .WithTopic($"fanuc/{machine.Id}-all/PING")
                                .WithPayload(JObject.FromObject(payload).ToString())
                                .WithRetainFlag()
                                .Build();

                            var r = mqtt.PublishAsync(msg, CancellationToken.None).Result;
                        }
                    }
                }
            }
        }
    }
}      