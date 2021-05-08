using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace fanuc.collectors
{
    public class Info1 : Collector
    {
        public Info1(Machine machine) : base(machine)
        {
            #if ARMV7
            Console.WriteLine("ARMV7 - Focas1.cnc_startupprocess()");
            machine.Platform.StartupProcess();
            #endif
        }
        
        public override void Initialize()
        {
            while (!_machine.VeneersCreated)
            {
                Console.WriteLine("fanuc - creating veneers");

                dynamic connect = _machine.Platform.Connect();
                Console.WriteLine(JObject.FromObject(connect).ToString());

                if (connect.success)
                {
                    _machine.AddVeneer(typeof(fanuc.veneers.Connect), "connect");
                    _machine.AddVeneer(typeof(fanuc.veneers.SysInfo), "sys_info");
                    
                    dynamic disconnect = _machine.Platform.Disconnect();
                    _machine.VeneersCreated = true;

                    Console.WriteLine("fanuc - created veneers");
                }
                else
                {
                    // not in here
                    System.Threading.Thread.Sleep(2000);
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

                dynamic disconnect = _machine.Platform.Disconnect();

                LastSuccess = connect.success;
            }
        }
    }
}