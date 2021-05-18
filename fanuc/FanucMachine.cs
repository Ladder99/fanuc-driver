using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using l99.driver.@base;

namespace l99.driver.fanuc
{
    public class FanucMachine: Machine
    {
        public override string ToString()
        {
            return new
            {
                Id,
                _focasEndpoint.IPAddress,
                _focasEndpoint.Port,
                _focasEndpoint.ConnectionTimeout
            }.ToString();
        }

        public override dynamic Info
        {
            get
            {
                return new
                {
                    _id,
                    _focasEndpoint.IPAddress,
                    _focasEndpoint.Port,
                    _focasEndpoint.ConnectionTimeout
                };
            }
        }

        public FocasEndpoint FocasEndpoint
        {
            get => _focasEndpoint;
        }
        
        private FocasEndpoint _focasEndpoint;
        
        public Platform Platform
        {
            get => _platform;
        }
        
        private Platform _platform;
        
        public FanucMachine(Machines machines, bool enabled, string id, object config) : base(machines, enabled, id, config)
        {
            dynamic cfg = (dynamic) config;
            _focasEndpoint = new FocasEndpoint(cfg.focasIpAddress, cfg.focasPort, cfg.timeout);
            _platform = new Platform(this);
        }
    }
}