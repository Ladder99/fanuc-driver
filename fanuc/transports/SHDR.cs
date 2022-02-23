using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;
using MTConnect.Adapters.Shdr;
using NLog.Targets;
using Scriban;

namespace l99.driver.fanuc.transports
{
    public class SHDR : Transport
    {
        private class ConfigDataItem
        {
            public string Id;
            public string TransformFormat;
            public Template TransformTemplate;
            public bool TemplateHasErrors => TransformTemplate?.HasErrors ?? true;
        }
        
        private dynamic _config;

        private List<ConfigDataItem> _dataItems = new List<ConfigDataItem>();

        private Dictionary<string, Dictionary<string, dynamic>> _observations = new Dictionary<string, Dictionary<string, dynamic>>();
        
        private ShdrAdapter _adapter;

        public SHDR(Machine machine, object cfg) : base(machine, cfg)
        {
            _config = cfg;
        }

        public override async Task<dynamic?> CreateAsync()
        {
            _dataItems = (_config.transport["dataitems"] as List<dynamic>)
                .ConvertAll(
                    o => new ConfigDataItem() { 
                        Id = (string) o["id"],
                        TransformFormat = (string) o["transform"],
                        TransformTemplate = Template.Parse((string) o["transform"])
                    });

            if (_dataItems.Any(di => di.TemplateHasErrors))
            {
                logger.Error($"[{machine.Id}] Some templates have errors.");
            }
            
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
                            new Dictionary<string, dynamic>());
                    }

                    if (!_observations[veneer.Name].ContainsKey(sliceKey))
                    {
                        _observations[veneer.Name].Add(
                            sliceKey, new { 
                                data.observation, 
                                data.state });
                    }
                    else
                    {
                        _observations[veneer.Name][sliceKey] = new { data.observation, data.state };
                    }
                    
                    break;
                
                case "SWEEP_END":

                    if (!_observations.ContainsKey("sweep"))
                    {
                        _observations.Add(
                            "sweep",
                            new Dictionary<string, dynamic>());
                        
                        _observations["sweep"].Add(
                            "", new { data.observation, data.state });
                    }
                    else
                    {
                        _observations["sweep"][""] = new { data.observation, data.state };
                    }

                    await renderAllAsync();
                    
                    break;
                
                case "INT_MODEL":

                    break;
            }
        }

        private async Task renderAllAsync()
        {
            foreach (var dataitem in _dataItems)
            {
                Console.WriteLine("------");
                Console.WriteLine(dataitem.Id);
                Console.WriteLine(dataitem.TransformFormat);
                var a = await dataitem.TransformTemplate.RenderAsync(new {observations = _observations});
                Console.WriteLine(a);
            }
        }
    }
}