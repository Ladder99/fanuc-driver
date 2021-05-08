namespace fanuc.collectors
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
            machine.Platform.StartupProcess(0, "~/focas2.log");
        }

        ~Collector()
        {
            // TODO: verify inocation
            _machine.Platform.ExitProcess();
        }
        
        public virtual void Initialize()
        {
            
        }

        public virtual void Collect()
        {
            
        }
    }
}