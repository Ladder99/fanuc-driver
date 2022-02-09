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
        
        protected dynamic PathMarker(dynamic path)
        {
            return new { path.request.cnc_setpath.path_no };
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

        protected dynamic spindleMarker(dynamic spindle)
        {
            return new
            {
                name = ((char)spindle.name).AsAscii(), 
                suff1 =  ((char)spindle.suff1).AsAscii(),
                suff2 =  ((char)spindle.suff2).AsAscii()
                // suff3 =  ((char)spindle.suff3).AsAscii() reserved
            };
        }

        protected dynamic axisMarker(dynamic axis)
        {
            return new
            {
                name = ((char)axis.name).AsAscii(), 
                suff =  ((char)axis.suff).AsAscii()
            };
        }
    }
}