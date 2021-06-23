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
                    _id = id,
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
        
        public FanucMachine(Machines machines, bool enabled, string id, object config) : base(machines, enabled, id, config)
        {
            dynamic cfg = (dynamic) config;
            _focasEndpoint = new FocasEndpoint(cfg.ip, (ushort)cfg.port, (short)cfg.timeout);
            this["platform"] = new Platform(this);
        }
    }
}