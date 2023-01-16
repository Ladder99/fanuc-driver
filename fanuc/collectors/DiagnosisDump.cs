using l99.driver.fanuc.strategies;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.collectors
{
    // ReSharper disable once UnusedType.Global
    public class DiagnosisDump : FanucMultiStrategyCollector
    {
        public DiagnosisDump(FanucMultiStrategy strategy) : base(strategy)
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
                Console.WriteLine("*** DIAGNOSIS DUMP ***");
                
                var diagnum = await Strategy.Platform.RdDiagNumAsync();
                var diagnum_inner = diagnum.response.cnc_rddiagnum.diagnum;
                
                Console.WriteLine($"Minimum: {diagnum_inner.diag_min}, " +
                                  $"Maximum: {diagnum_inner.diag_max}, " +
                                  $"Total:{diagnum_inner.total_no}");

                bool all_done = false;
                short start = (short) diagnum_inner.diag_min;
                
                while(!all_done)
                {
                    var diaginfo = await Strategy.Platform.RdDiagInfoAsync(start, 10);
                    var diaginfo_inner = diaginfo.response.cnc_rddiaginfo.diagif;
                    
                    if (diaginfo_inner.next_no < start)
                        all_done = true;

                    start = diaginfo_inner.next_no;
                    
                    var fields = diaginfo_inner.info.GetType().GetFields();

                    for (int i = 0; i <= diaginfo_inner.info_no - 1; i++)
                    {
                        var value = fields[i].GetValue(diaginfo_inner.info);
                        
                        Console.WriteLine($"Diag: {value.diag_no}, Type: {value.diag_type}");
                    }
                }
                
                Console.Write("*** *** **** ***");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"[{Strategy.Machine.Id}] Diagnosis Dump Failed!");
            }
        }
    }
}