using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;
using l99.driver.@base.mqtt;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace l99.driver.fanuc
{
    partial class Program
    {
        private static ILogger _logger;
        
        static async Task Main(string[] args)
        {
            string nlog_file = getArgument(args, "--nlog", "nlog.config");
            _logger = setupLogger(nlog_file);
            string config_file = getArgument(args, "--config", "config.yml");
            dynamic config = readConfig(config_file);
            Machines machines = await createMachines(config);
            await machines.RunAsync();
            LogManager.Shutdown();
        }

        static string getArgument(string[] args, string option, string defaultValue)
        {
            var value = args.SkipWhile(i => i != option).Skip(1).Take(1).FirstOrDefault();
            var config_path = string.IsNullOrEmpty(value) ? defaultValue : value;
            Console.WriteLine($"Argument '{option}' = '{config_path}'");
            return config_path;
        }
        
        static Logger setupLogger(string config_file)
        {
            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(config_file);

            var config = new ConfigurationBuilder().Build();

            return LogManager.Setup()
                .SetupExtensions(ext => ext.RegisterConfigSettings(config))
                .GetCurrentClassLogger();
        }

        static dynamic readConfig(string config_file)
        {
            var input = new StreamReader(config_file);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var config = deserializer.Deserialize(input);
            _logger.Trace($"Deserialized configuration:\n{JObject.FromObject(config).ToString()}");
            return config;
        }

        static async Task<Machines> createMachines(dynamic config)
        {
            var machine_confs = new List<dynamic>();

            foreach (dynamic machine_conf in config["machines"])
            {
                var built_config = new
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
                        port = machine_conf["broker"]["net_port"],
                        auto_connect = machine_conf["broker"]["auto_connect"]
                    }
                };
                
                _logger.Trace($"Machine configuration built:\n{JObject.FromObject(built_config).ToString()}");
                
                machine_confs.Add(built_config);
            }

            Brokers brokers = new Brokers();
            Machines machines = new Machines();
            
            foreach (var cfg in machine_confs)
            {
                _logger.Trace($"Creating machine from config:\n{JObject.FromObject(cfg).ToString()}");
                Broker broker = await brokers.AddAsync(cfg.broker);
                broker["disco"] = new Disco();
                Machine machine = machines.Add(cfg.machine);
                machine["broker"] = broker;
                machine.AddCollector(Type.GetType(cfg.machine.collector), cfg.machine.collectorSweepMs);
                await machine.AddHandlerAsync(Type.GetType(cfg.machine.handler));
            }

            return machines;
        }
    }
}      