using l99.driver.fanuc.strategies;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors
{
    // ReSharper disable once UnusedType.Global
    public class ParameterDump : FanucMultiStrategyCollector
    {
        public ParameterDump(FanucMultiStrategy strategy) : base(strategy)
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
                Console.WriteLine("*** PARAMETER DUMP ***");
                
                var paranum = await Strategy.Platform.RdParaNumAsync();
                var paranum_inner = paranum.response.cnc_rdparanum.paranum;
                
                Console.WriteLine($"Minimum: {paranum_inner.para_min}, " +
                                  $"Maximum: {paranum_inner.para_max}, " +
                                  $"Total:{paranum_inner.total_no}");

                bool all_done = false;
                short start = (short) paranum_inner.para_min;
                
                while(!all_done)
                {
                    var parainfo = await Strategy.Platform.RdParaInfoAsync(start, 10);
                    var parainfo_inner = parainfo.response.cnc_rdparainfo.paraif;
                    
                    if (parainfo_inner.next_no < start)
                        all_done = true;

                    start = parainfo_inner.next_no;
                    
                    var fields = parainfo_inner.info.GetType().GetFields();

                    for (int i = 0; i <= parainfo_inner.info_no - 1; i++)
                    {
                        var value = fields[i].GetValue(parainfo_inner.info);
                        
                        Console.WriteLine($"Param: {value.prm_no}, Type: {value.prm_type}");
                    }
                }
                
                Console.Write("*** *** **** ***");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"[{Strategy.Machine.Id}] Parameter Dump Failed!");
            }
        }
    }
}