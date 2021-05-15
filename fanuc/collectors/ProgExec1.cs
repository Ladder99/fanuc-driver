using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace fanuc.collectors
{
    public class ProgExec1 : Collector
    {
        public ProgExec1(Machine machine, int sweepMs = 1000) : base(machine, sweepMs)
        {
            
        }
        
        public override void Initialize()
        {
            while (!_machine.VeneersApplied)
            {
                Console.WriteLine("fanuc - creating veneers");

                dynamic connect = _machine.Platform.Connect();
                Console.WriteLine(JObject.FromObject(connect).ToString());

                if (connect.success)
                {
                    _machine.ApplyVeneer(typeof(fanuc.veneers.Connect), "connect");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.SysInfo), "sys_info");
                    
                    dynamic disconnect = _machine.Platform.Disconnect();
                    _machine.VeneersApplied = true;

                    Console.WriteLine("fanuc - created veneers");
                }
                else
                {
                    // not in here
                    System.Threading.Thread.Sleep(_sweepMs);
                }
            }
        }

        public override void Collect()
        {
            dynamic connect = _machine.Platform.Connect();
            _machine.PeelVeneer("connect", connect);

            if (connect.success)
            {
                dynamic info = _machine.Platform.SysInfo();
                _machine.PeelVeneer("sys_info", info);

                dynamic seqnum = _machine.Platform.RdSeqNum();
                dynamic prgname = _machine.Platform.ExePrgName();
                dynamic prgname2 = _machine.Platform.ExePrgName2();
                
                dynamic disconnect = _machine.Platform.Disconnect();

                LastSuccess = connect.success;
            }
        }
    }
}