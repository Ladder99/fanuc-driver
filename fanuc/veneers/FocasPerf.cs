using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;
using MoreLinq;

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
            var max = ((List<dynamic>)input.focas_invocations).MaxBy(o => o.invocationMs).First();
            var min = ((List<dynamic>)input.focas_invocations).MinBy(o => o.invocationMs).First();
            var avg = (int)((List<dynamic>)input.focas_invocations).Average(o => (int)o.invocationMs);
            var sum = ((List<dynamic>) input.focas_invocations).Sum(o => (int)o.invocationMs);
            var failedMethods = ((List<dynamic>) input.focas_invocations)
                .Where(o => o.rc != 0)
                .Select(o => new { o.method, o.rc });
            
            var current_value = new
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
                
            await onDataArrivedAsync(input, current_value);
                
            return new { veneer = this };
        }
    }
}