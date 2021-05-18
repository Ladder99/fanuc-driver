using System;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.collectors
{
    public class FanucCollector : Collector
    {
        public FanucCollector(Machine machine, int sweepMs = 1000) : base(machine, sweepMs)
        {
            _machine["platform"].StartupProcess(3, "~/focas2.log");
        }
        
        ~FanucCollector()
        {
            // TODO: verify inocation
            _machine["platform"].ExitProcess();
        }
    }
}