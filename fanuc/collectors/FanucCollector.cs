using System;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.collectors
{
    public class FanucCollector : Collector
    {
        protected dynamic _platform;
        
        public FanucCollector(Machine machine, int sweepMs = 1000) : base(machine, sweepMs)
        {
            _platform = _machine["platform"];
            _platform.StartupProcess(3, "~/focas2.log");
        }
        
        ~FanucCollector()
        {
            // TODO: verify invocation
            _platform.ExitProcess();
        }
        
        protected dynamic PathMarker(dynamic path)
        {
            return new {path.request.cnc_setpath.path_no};
        }

        protected dynamic AxisName(dynamic axis)
        {
            return ((char) axis.name).AsAscii() + ((char) axis.suff).AsAscii();
        }

        protected dynamic SpindleName(dynamic spindle)
        {
            return ((char) spindle.name).AsAscii() +
                   ((char) spindle.suff1).AsAscii() +
                   ((char) spindle.suff2).AsAscii();
                   // ((char) spindle.suff3).AsAscii(); reserved
        }

        protected dynamic SpindleMarker(dynamic spindle)
        {
            return new
            {
                name = ((char)spindle.name).AsAscii(), 
                suff1 =  ((char)spindle.suff1).AsAscii(),
                suff2 =  ((char)spindle.suff2).AsAscii()
                // suff3 =  ((char)spindle.suff3).AsAscii() reserved
            };
        }

        protected dynamic AxisMarker(dynamic axis)
        {
            return new
            {
                name = ((char)axis.name).AsAscii(), 
                suff =  ((char)axis.suff).AsAscii()
            };
        }
    }
}