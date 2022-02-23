using l99.driver.@base;

namespace l99.driver.fanuc.strategies
{
    public class FanucStrategy : Strategy
    {
        protected dynamic platform;
        
        public FanucStrategy(Machine machine, object cfg) : base(machine, cfg)
        {
            platform = base.machine["platform"];
            platform.StartupProcess(3, "focas2.log");
        }
        
        ~FanucStrategy()
        {
            // TODO: verify invocation
            platform.ExitProcess();
        }
        
        protected dynamic PathMarker(short number)
        {
            return new
            {
                type = "path",
                number
            };
        }

        protected dynamic axisName(dynamic axis)
        {
            return ((char) axis.name).AsAscii() + ((char) axis.suff).AsAscii();
        }

        protected dynamic spindleName(dynamic spindle)
        {
            return ((char) spindle.name).AsAscii() +
                   ((char) spindle.suff1).AsAscii() +
                   ((char) spindle.suff2).AsAscii();
                   // ((char) spindle.suff3).AsAscii(); reserved
        }

        protected dynamic spindleMarker(short number, string name)
        {
            return new
            {
                type = "spindle",
                number,
                name
            };
        }

        protected dynamic axisMarker(short number, string name)
        {
            return new
            {
                type = "axis",
                number,
                name
            };
        }
    }
}