using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;
using JsonFlatten;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.transports
{
    public class SpB : Transport
    {
        private dynamic _config;
        
        public SpB(Machine machine, object cfg) : base(machine, cfg)
        {
            _config = cfg;
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

            switch (@event)
            {
                case "DATA_ARRIVE":

                    JObject jc =  JObject.FromObject(data);
                    var fc = jc.Flatten();

                    break;
                    
                case "SWEEP_END":
                    
                    JObject j =  JObject.FromObject(data);
                    var f = j.Flatten();
                    
                    break;
                
                case "INT_MODEL":

                    break;
            }
        }

    }
}
