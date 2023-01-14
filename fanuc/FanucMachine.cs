using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FanucMachine: Machine
    {
        public override string ToString()
        {
#pragma warning disable CS8603
            return new
            {
                Id,
                _focasEndpoint.IPAddress,
                _focasEndpoint.Port,
                _focasEndpoint.ConnectionTimeout
            }.ToString();
#pragma warning restore CS8603
        }

        public override dynamic Info =>
            new
            {
                _id = Id,
                _focasEndpoint.IPAddress,
                _focasEndpoint.Port,
                _focasEndpoint.ConnectionTimeout
            };

        public FocasEndpoint FocasEndpoint => _focasEndpoint;

        private readonly FocasEndpoint _focasEndpoint;
        
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