using System;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.collectors
{
    public class ServoData : FanucCollector
    {
        public ServoData(Machine machine, int sweepMs = 1000) : base(machine, sweepMs)
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
                        dynamic path = await _machine["platform"].SetPathAsync(1);
                        dynamic x = await _machine["platform"].SvdtStartRdAsync(1);
                        
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
                
                if (connect.success)
                {
                    dynamic x = await _machine["platform"].SvdtRdDataAsync(1024);
                    
                    dynamic disconnect = await _machine["platform"].DisconnectAsync();
                }
                
                LastSuccess = connect.success;

                
                
                LastSuccess = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[{_machine.Id}] Collector sweep failed.");
            }

            return null;
        }
    }
}