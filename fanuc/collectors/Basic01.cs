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
        
        public override async Task<dynamic?> InitializeAsync()
        {
            try
            {
                while (!_machine.VeneersApplied)
                {
                    dynamic connect = await _machine["platform"].ConnectAsync();
                    
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
                    }
                    else
                    {
                        await Task.Delay(_sweepMs);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[{_machine.Id}] Collector initialization failed.");
            }

            return null;
        }
   
        public override async Task<dynamic?> CollectAsync()
        {
            try
            {
                dynamic connect = await _machine["platform"].ConnectAsync();
                await _machine.PeelVeneerAsync("connect", connect);

                if (connect.success)
                {
                    dynamic cncid = await _machine["platform"].CNCIdAsync();
                    await _machine.PeelVeneerAsync("cnc_id", cncid);

                    dynamic poweron = await _machine["platform"].RdTimerAsync(0);
                    await _machine.PeelVeneerAsync("power_on_time", poweron);

                    dynamic poweron_6750 = await _machine["platform"].RdParamDoubleWordNoAxisAsync(6750);
                    await _machine.PeelVeneerAsync("power_on_time_6750", poweron_6750);

                    dynamic info = await _machine["platform"].SysInfoAsync();
                    await _machine.PeelVeneerAsync("sys_info", info);

                    dynamic paths = await _machine["platform"].GetPathAsync();
                    await _machine.PeelVeneerAsync("get_path", paths);

                    dynamic disconnect = await _machine["platform"].DisconnectAsync();
                }

                LastSuccess = connect.success;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[{_machine.Id}] Collector sweep failed.");
            }

            return null;
        }
    }
}