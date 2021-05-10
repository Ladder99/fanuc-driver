using System;
using Newtonsoft.Json.Linq;

namespace fanuc.collectors
{
    public class Basic01 : Collector
    {
        public Basic01(Machine machine, int sweepMs = 1000) : base(machine, sweepMs)
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
                    _machine.ApplyVeneer(typeof(fanuc.veneers.CNCId), "cnc_id");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.RdTimer), "power_on_time");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.RdParamLData), "power_on_time_6750");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.SysInfo), "sys_info");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.GetPath), "get_path");
                    
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
                dynamic cncid = _machine.Platform.CNCId();
                _machine.PeelVeneer("cnc_id", cncid);
                
                dynamic poweron = _machine.Platform.RdTimer(0);
                _machine.PeelVeneer("power_on_time", poweron);
                
                dynamic poweron_6750 = _machine.Platform.RdParam(6750, 0, 8, 1);
                _machine.PeelVeneer("power_on_time_6750", poweron_6750);
                
                dynamic info = _machine.Platform.SysInfo();
                _machine.PeelVeneer("sys_info", info);
                
                dynamic paths = _machine.Platform.GetPath();
                _machine.PeelVeneer("get_path", paths);

                dynamic disconnect = _machine.Platform.Disconnect();
            }
            
            LastSuccess = connect.success;
        }
    }
}