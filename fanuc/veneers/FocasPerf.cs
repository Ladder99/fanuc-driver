using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;

namespace fanuc.veneers
{
    public class FocasPerf : Veneer
    {
        public FocasPerf(string name = "") : base(name)
        {
            _lastChangedValue = new
            {
                invocation = new
                {
                    maxMethod = string.Empty,
                    maxMs = -1,
                    minMs = -1,
                    avgMs = -1
                }
            };
        }
        
        protected override dynamic Any(dynamic input)
        {
            if (input.success)
            {
                var max = ((List<dynamic>)input).MaxBy(o => o.invocationMs).First();
                var min = ((List<dynamic>)input).MinBy(o => o.invocationMs).First();
                var avg = (int)((List<dynamic>) input).Average(o => o.invocationMs);
                var current_value = new
                {
                    invocation = new
                    {
                        maxMethod = max.method,
                        maxMs = max.invocationMs,
                        minMs = min.invocationMs,
                        avgMs = avg
                    }
                };;
                
                this.onDataArrived(input, current_value);
                
                if (!current_value.Equals(this._lastChangedValue))
                {
                    this.onDataChanged(input, current_value);
                }
            }
            else
            {
                onError(input);
            }

            return new { veneer = this };
        }
    }
}