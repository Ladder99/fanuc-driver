using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;
using MTConnect.Adapters.Shdr;

namespace l99.driver.fanuc.transports
{
    public class SHDR : Transport
    {
        private dynamic _config;

        private Dictionary<string, dynamic> _dataItems = new Dictionary<string, dynamic>();

        private Dictionary<string, Dictionary<string, (dynamic, dynamic)>> _observations = new Dictionary<string, Dictionary<string, (dynamic, dynamic)>>();
        
        private ShdrAdapter _adapter;

        public SHDR(Machine machine, object cfg) : base(machine, cfg)
        {
            _config = cfg;
        }

        public override async Task<dynamic?> CreateAsync()
        {
            _dataItems = (_config.transport["dataitems"] as List<dynamic>)
                .ToDictionary(
                    o => (string) o["id"],
                    o => o);
            
            _adapter = new ShdrAdapter(
                _config.transport["device_name"],
                _config.transport["net"]["port"],
                _config.transport["net"]["heartbeat_ms"]);
            
            _adapter.Start();
            
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
            
            switch (@event)
            {
                case "DATA_ARRIVE":
                case "DATA_CHANGE":

                    var sliceKey = veneer.SliceKey == null ? "" : veneer.SliceKey.ToString();
                    
                    if (!_observations.ContainsKey(veneer.Name))
                    {
                        _observations.Add(
                            veneer.Name,
                            new Dictionary<string, (dynamic, dynamic)>());
                    }
                    
                    if (!_observations[veneer.Name].ContainsKey(sliceKey))
                    {
                        _observations[veneer.Name].Add(
                            sliceKey, (data.observation, data.state));
                    }
                    else
                    {
                        _observations[veneer.Name][sliceKey] = (data.observation, data.state);
                    }
                    
                    break;
                
                case "SWEEP_END":

                    if (!_observations.ContainsKey("sweep"))
                    {
                        _observations.Add(
                            "sweep",
                            new Dictionary<string, (dynamic, dynamic)>());
                        
                        _observations["sweep"].Add(
                            "", (data.observation, data.state));
                    }
                    else
                    {
                        _observations["sweep"][""] = (data.observation, data.state);
                    }
                    
                    break;
                
                case "INT_MODEL":

                    break;
            }
        }
    }
}