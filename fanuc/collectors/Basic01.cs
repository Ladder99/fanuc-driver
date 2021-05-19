using System;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.collectors
{
    public class Basic01 : FanucCollector
    {
        public Basic01(Machine machine, int sweepMs = 1000) : base(machine, sweepMs)
        {
            
        }
        
        public override void Initialize()
        {
            while (!_machine.VeneersApplied)
            {
                Console.WriteLine("fanuc - creating veneers");

                dynamic connect = _machine["platform"].Connect();
                Console.WriteLine(JObject.FromObject(connect).ToString());

                if (connect.success)
                {
                    _machine.ApplyVeneer(typeof(fanuc.veneers.Connect), "connect");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.CNCId), "cnc_id");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.RdTimer), "power_on_time");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.RdParamLData), "power_on_time_6750");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.SysInfo), "sys_info");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.GetPath), "get_path");
                    
                    dynamic disconnect = _machine["platform"].Disconnect();
                    
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

        public override async Task InitializeAsync()
        {
            while (!_machine.VeneersApplied)
            {
                Console.WriteLine("fanuc - creating veneers");

                dynamic connect = await _machine["platform"].ConnectAsync();
                Console.WriteLine(JObject.FromObject(connect).ToString());

                if (connect.success)
                {
                    _machine.ApplyVeneer(typeof(fanuc.veneers.Connect), "connect");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.CNCId), "cnc_id");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.RdTimer), "power_on_time");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.RdParamLData), "power_on_time_6750");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.SysInfo), "sys_info");
                    _machine.ApplyVeneer(typeof(fanuc.veneers.GetPath), "get_path");
                    
                    dynamic disconnect = await _machine["platform"].DisconnectAsync();
                    
                    _machine.VeneersApplied = true;

                    Console.WriteLine("fanuc - created veneers");
                }
                else
                {
                    // not in here
                    Task.Delay(_sweepMs);
                }
            }
        }

        public override void Collect()
        {
            dynamic connect = _machine["platform"].Connect();
            _machine.PeelVeneer("connect", connect);

            if (connect.success)
            {
                dynamic cncid = _machine["platform"].CNCId();
                _machine.PeelVeneer("cnc_id", cncid);
                
                dynamic poweron = _machine["platform"].RdTimer(0);
                _machine.PeelVeneer("power_on_time", poweron);
                
                dynamic poweron_6750 = _machine["platform"].RdParam(6750, 0, 8, 1);
                _machine.PeelVeneer("power_on_time_6750", poweron_6750);
                
                dynamic info = _machine["platform"].SysInfo();
                _machine.PeelVeneer("sys_info", info);
                
                dynamic paths = _machine["platform"].GetPath();
                _machine.PeelVeneer("get_path", paths);

                dynamic disconnect = _machine["platform"].Disconnect();
            }
            
            LastSuccess = connect.success;
        }
        
        public override async Task CollectAsync()
        {
            dynamic connect = await _machine["platform"].ConnectAsync();
            _machine.PeelVeneer("connect", connect);

            if (connect.success)
            {
                dynamic cncid = await _machine["platform"].CNCIdAsync();
                _machine.PeelVeneer("cnc_id", cncid);
                
                dynamic poweron = await _machine["platform"].RdTimerAsync(0);
                _machine.PeelVeneer("power_on_time", poweron);
                
                dynamic poweron_6750 = await _machine["platform"].RdParamAsync(6750, 0, 8, 1);
                _machine.PeelVeneer("power_on_time_6750", poweron_6750);
                
                dynamic info = await _machine["platform"].SysInfoAsync();
                _machine.PeelVeneer("sys_info", info);
                
                dynamic paths = await _machine["platform"].GetPathAsync();
                _machine.PeelVeneer("get_path", paths);

                dynamic disconnect = await _machine["platform"].DisconnectAsync();
            }
            
            LastSuccess = connect.success;
        }
    }
}