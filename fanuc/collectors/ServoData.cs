using System;
using System.Threading.Tasks;
using l99.driver.@base;
using Newtonsoft.Json.Linq;

namespace l99.driver.fanuc.collectors
{
    public class ServoData : FanucCollector
    {
        public ServoData(Machine machine, int sweepMs = 1000, params dynamic[] additionalParams) : base(machine, sweepMs, additionalParams)
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
                        dynamic path = await machine["platform"].SetPathAsync(1);
                        dynamic x = await machine["platform"].SvdtStartRdAsync(1);
                        
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
                
                if (connect.success)
                {
                    dynamic x = await machine["platform"].SvdtRdDataAsync(1024);
                    
                    dynamic disconnect = await machine["platform"].DisconnectAsync();
                }
                
                LastSuccess = connect.success;

                
                
                LastSuccess = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[{machine.Id}] Collector sweep failed.");
            }

            return null;
        }
    }
}