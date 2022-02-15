using System;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.transports
{
    public class SHDR : Transport
    {
        private dynamic _config;

        public SHDR(Machine machine, object cfg) : base(machine, cfg)
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
            
        }
    }
}