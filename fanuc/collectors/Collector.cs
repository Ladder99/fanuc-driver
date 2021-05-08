namespace fanuc.collectors
{
    public class Collector
    {
        protected Machine _machine;
        
        public bool LastSuccess { get; set; }
        
        public Collector(Machine machine)
        {
            _machine = machine;
        }
        
        public virtual void Initialize()
        {
            
        }

        public virtual void Collect()
        {
            
        }
    }
}