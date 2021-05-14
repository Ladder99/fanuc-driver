using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace fanuc
{
    partial class Program
    {
        static void Main(string[] args)
        {
            string config_file = getArgument(args, "--config", "config.yml");
            dynamic config = readConfig(config_file);
            Machines machines = createMachines(config);
            machines.Run();
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
        
        static Machines createMachines(dynamic config)
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
                    collectorSweepMs = machine_conf["sweep_ms"],
                    handler = machine_conf["handler_type"],
                    
                    mqtt_enabled = machine_conf["broker"]["enabled"],
                    mqtt_pub_status = machine_conf["broker"]["publish_status"],
                    mqtt_pub_arrivals = machine_conf["broker"]["publish_arrivals"],
                    mqtt_pub_changes = machine_conf["broker"]["publish_changes"],
                    mqtt_ip = machine_conf["broker"]["net_ip"], 
                    mqtt_port = machine_conf["broker"]["net_port"]
                });
            }

            Machines machines = new Machines();
            
            // init
            // var mqtt_disco = new MQTTDisco(mqtt, config["broker"]);
            
            // before arrival
            // mqtt_disco.Add(vv.Machine.Id);
            
            // before change
            // mqtt_disco.Add(vv.Machine.Id);
            
            foreach (var cfg in machine_confs)
            {
                Machine machine = machines.Add(cfg.enabled, cfg.id, cfg.ip, (ushort)cfg.port, (short)cfg.timeout);
                machine.AddCollector(Type.GetType(cfg.collector), cfg.collectorSweepMs);
                machine.AddHandler(Type.GetType(cfg.handler), cfg);
            }

            return machines;
        }
    }
}      