using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;
using l99.driver.@base.mqtt;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace l99.driver.fanuc
{
    partial class Program
    {
        static async Task Main(string[] args)
        {
            string config_file = getArgument(args, "--config", "config.yml");
            dynamic config = readConfig(config_file);
            Machines machines = createMachines(config);
            await machines.RunAsync();
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
                    machine = new {
                        enabled = machine_conf["enabled"],
                        type = machine_conf["type"],
                        id = machine_conf["id"],
                        ip = machine_conf["net_ip"],
                        port = machine_conf["net_port"],
                        timeout = machine_conf["net_timeout_s"],
                        collector = machine_conf["strategy_type"],
                        collectorSweepMs = machine_conf["sweep_ms"],
                        handler = machine_conf["handler_type"]
                    },
                    broker = new
                    {
                        enabled = machine_conf["broker"]["enabled"],
                        pub_status = machine_conf["broker"]["publish_status"],
                        pub_arrivals = machine_conf["broker"]["publish_arrivals"],
                        pub_changes = machine_conf["broker"]["publish_changes"],
                        ip = machine_conf["broker"]["net_ip"], 
                        port = machine_conf["broker"]["net_port"]
                    }
                });
            }

            Brokers brokers = new Brokers();
            Machines machines = new Machines();
            
            foreach (var cfg in machine_confs)
            {
                Broker broker = brokers.Add(cfg.broker);
                broker["disco"] = new Disco();
                Machine machine = machines.Add(cfg.machine);
                machine["broker"] = broker;
                machine.AddCollector(Type.GetType(cfg.machine.collector), cfg.machine.collectorSweepMs);
                machine.AddHandler(Type.GetType(cfg.machine.handler));
            }

            return machines;
        }
    }
}      