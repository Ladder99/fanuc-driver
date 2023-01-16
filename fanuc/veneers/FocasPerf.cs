using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers
{
    public class FocasPerf : Veneer
    {
        public FocasPerf(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                sweepMs = -1,
                invocation = new
                {
                    count = -1,
                    maxMethod = string.Empty,
                    maxMs = -1,
                    minMs = -1,
                    avgMs = -1,
                    failedMethods = new List<string>()
                }
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            // TODO : review extension usage
            //  https://stackoverflow.com/questions/69917366/net6-morelinq-call-is-ambiguous-between-system-linq-enumerable-distinctby-a
            var max = MoreEnumerable
                .MaxBy(((List<dynamic>)input.focas_invocations), o => o.invocationMs).First();
            var min =  MoreEnumerable
                .MinBy(((List<dynamic>)input.focas_invocations), o => o.invocationMs).First();
            var avg = (int)((List<dynamic>)input.focas_invocations).Average(o => (int)o.invocationMs);
            var sum = ((List<dynamic>) input.focas_invocations).Sum(o => (int)o.invocationMs);
            var failedMethods = ((List<dynamic>) input.focas_invocations)
                .Where(o => o.rc != 0)
                .Select(o => new { o.method, o.rc });
            
            var currentValue = new
            {
                sweep_ms = input.sweepMs,
                invocation = new
                {
                    count = input.focas_invocations.Count,
                    max_method = max.method,
                    max_ms = max.invocationMs,
                    min_ms = min.invocationMs,
                    avg_ms = avg,
                    sum_ms = sum,
                    failed_methods = failedMethods
                }
            };;
                
            await OnDataArrivedAsync(input, currentValue);
                
            return new { veneer = this };
        }
    }
}