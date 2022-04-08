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
            dynamic cfg = config;
            this["platform"] = new Platform(this);
            
            //TODO: validate config
            _focasEndpoint = new FocasEndpoint(
                cfg.type["net"]["ip"], 
                (ushort)cfg.type["net"]["port"], 
                (short)cfg.type["net"]["timeout_s"]);
            
        }
    }
}