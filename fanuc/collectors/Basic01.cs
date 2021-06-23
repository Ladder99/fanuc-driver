using System;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.collectors
{
    public class Basic01 : FanucCollector
    {
        public Basic01(Machine machine, int sweepMs = 1000, params dynamic[] additionalParams) : base(machine, sweepMs, additionalParams)
        {
            
        }
        
        public override async Task<dynamic?> InitializeAsync()
        {
            try
            {
                while (!machine.VeneersApplied)
                {
                    dynamic connect = await machine["platform"].ConnectAsync();
                    
                    if (connect.success)
                    {
                        machine.ApplyVeneer(typeof(fanuc.veneers.Connect), "connect");
                        machine.ApplyVeneer(typeof(fanuc.veneers.CNCId), "cnc_id");
                        machine.ApplyVeneer(typeof(fanuc.veneers.RdTimer), "power_on_time");
                        machine.ApplyVeneer(typeof(fanuc.veneers.RdParamLData), "power_on_time_6750");
                        machine.ApplyVeneer(typeof(fanuc.veneers.SysInfo), "sys_info");
                        machine.ApplyVeneer(typeof(fanuc.veneers.GetPath), "get_path");

                        dynamic disconnect = await machine["platform"].DisconnectAsync();

                        machine.VeneersApplied = true;
                    }
                    else
                    {
                        await Task.Delay(sweepMs);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[{machine.Id}] Collector initialization failed.");
            }

            return null;
        }
   
        public override async Task<dynamic?> CollectAsync()
        {
            try
            {
                dynamic connect = await machine["platform"].ConnectAsync();
                await machine.PeelVeneerAsync("connect", connect);

                if (connect.success)
                {
                    dynamic cncid = await machine["platform"].CNCIdAsync();
                    await machine.PeelVeneerAsync("cnc_id", cncid);

                    dynamic poweron = await machine["platform"].RdTimerAsync(0);
                    await machine.PeelVeneerAsync("power_on_time", poweron);

                    dynamic poweron_6750 = await machine["platform"].RdParamDoubleWordNoAxisAsync(6750);
                    await machine.PeelVeneerAsync("power_on_time_6750", poweron_6750);

                    dynamic info = await machine["platform"].SysInfoAsync();
                    await machine.PeelVeneerAsync("sys_info", info);

                    dynamic paths = await machine["platform"].GetPathAsync();
                    await machine.PeelVeneerAsync("get_path", paths);

                    dynamic disconnect = await machine["platform"].DisconnectAsync();
                }

                LastSuccess = connect.success;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[{machine.Id}] Collector sweep failed.");
            }

            return null;
        }
    }
}