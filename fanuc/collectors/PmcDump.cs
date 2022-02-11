using System;
using System.Threading.Tasks;
using l99.driver.fanuc.strategies;

namespace l99.driver.fanuc.collectors
{
    public class PmcDump : FanucMultiStrategyCollector
    {
        public PmcDump(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }

        private bool _dumped = false;

        public override async Task CollectRootAsync()
        {
            if (_dumped)
                return;

            _dumped = true;

            try
            {
                Console.WriteLine("*** PMC DUMP ***");
                
                var pmcinfo = await strategy.Platform.RdPmcInfoAsync();
                var pmcinfo_inner = pmcinfo.response.pmc_rdpmcinfo.pmcif;
                
                var fields = pmcinfo_inner.info.GetType().GetFields();

                for (int i = 0; i <= pmcinfo_inner.datano - 1; i++)
                {
                    var value = fields[i].GetValue(pmcinfo_inner.info);
                        
                    Console.WriteLine($"Type: {value.pmc_adr}, " +
                                      $"Attr: {value.adr_attr}, " +
                                      $"Top: {value.top_num}, " +
                                      $"Last: {value.last_num}");
                }
                
                Console.Write("*** *** **** ***");
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"[{strategy.Machine.Id}] PMC Dump Failed!");
            }
        }
    }
}