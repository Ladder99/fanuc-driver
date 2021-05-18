using System.Collections.Generic;

namespace l99.driver.@base.mqtt
{
    public class Brokers
    {
        private Dictionary<string, Broker> _brokers;
            
        public Brokers()
        {
            _brokers = new Dictionary<string, Broker>();
        }

        public Broker Add(dynamic cfg)
        {
            var key = cfg.ip + ":" + cfg.port;

            if (_brokers.ContainsKey(key))
            {
                return _brokers[key];
            }
            else
            {
                Broker broker = new Broker(cfg);
                broker.Connect();
                _brokers.Add(key, broker);
                return broker;
            }
        }
    }
}