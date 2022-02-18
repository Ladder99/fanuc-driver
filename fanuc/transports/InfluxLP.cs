using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using l99.driver.@base;
using Scriban;

namespace l99.driver.fanuc.transports
{
    public class InfluxLP : Transport
    {
        private dynamic _config;
        
        // config - veneer type, template text
        private Dictionary<string, string> _transformLookup = new Dictionary<string, string>();
        
        // runtime - veneer name, template
        private Dictionary<string, Template> _templateLookup = new Dictionary<string, Template>();

        private InfluxDBClient _client;

        public InfluxLP(Machine machine, object cfg) : base(machine, cfg)
        {
            _config = cfg;

            _client = InfluxDBClientFactory.Create(_config.transport["host"], _config.transport["token"]);
            
            _transformLookup = (_config.transport["transformers"] as Dictionary<dynamic,dynamic>)
                .ToDictionary(
                    kv => (string)kv.Key, 
                    kv => (string)kv.Value);
        }

        public override async Task<dynamic?> CreateAsync()
        {
            return null;
        }

        public override async Task ConnectAsync()
        {
            
        }

        public override async Task SendAsync(params dynamic[] parameters)
        {
            var @event = parameters[0];
            var veneer = parameters[1];
            var data = parameters[2];

            if (@event == "DATA_ARRIVAL" || @event == "DATA_CHANGE")
            {
                if (!_templateLookup.ContainsKey(veneer.Name))
                {
                    addTemplate(veneer);
                }

                if (_templateLookup.ContainsKey(veneer.Name))
                {
                    string lp = _templateLookup[veneer.Name].Render(new { data.observation, data.state.data });

                    //Console.WriteLine(lp);
                    
                    _client.GetWriteApiAsync()
                        .WriteRecordAsync(
                            _config.transport["bucket"],
                            _config.transport["org"],
                            WritePrecision.Ms,
                            lp);
                }
            }
        }

        void addTemplate(Veneer veneer)
        {
            string veneerFullType = $"{veneer.GetType().FullName}, {veneer.GetType().Assembly.GetName().Name}";

            if (_transformLookup.ContainsKey(veneerFullType))
            {
                string transform = _transformLookup[veneerFullType];

                if (!_templateLookup.ContainsKey(veneer.Name))
                {
                    _templateLookup.Add(veneer.Name, Template.Parse(transform));
                }
            }
        }
    }
}