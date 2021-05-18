namespace l99.driver.@base
{
    public class Collector
    {
        protected Machine _machine;
        protected int _sweepMs;
        public bool LastSuccess { get; set; }
        
        public Collector(Machine machine, int sweepMs = 1000)
        {
            _machine = machine;
            _sweepMs = sweepMs;
        }

        public virtual void Initialize()
        {
            
        }

        public virtual void Collect()
        {
            
        }
    }
}