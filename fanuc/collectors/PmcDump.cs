using l99.driver.fanuc.strategies;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors
{
    // ReSharper disable once UnusedType.Global
    public class PmcDump : FanucMultiStrategyCollector
    {
        public PmcDump(FanucMultiStrategy strategy) : base(strategy)
        {
            
        }

        private bool _dumped;

        public override async Task CollectRootAsync()
        {
            if (_dumped)
                return;

            _dumped = true;

            try
            {
                Console.WriteLine("*** PMC DUMP ***");
                
                var pmcinfo = await Strategy.Platform.RdPmcInfoAsync();
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
                Logger.Error(ex, $"[{Strategy.Machine.Id}] PMC Dump Failed!");
            }
        }
    }
}