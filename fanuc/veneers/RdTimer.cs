using System.Collections.Generic;
using System.Linq;

namespace fanuc.veneers
{
    public class RdTimer: Veneer
    {
        public RdTimer(string name = ""): base(name)
        {
            _lastValue = new 
            {
                minute = -1,
                msec = -1
            };
        }
        
        protected override dynamic Any(dynamic input)
        {
            if (input.success)
            {
                var current_value = new
                {
                    input.response.cnc_rdtimer.minute,
                    input.response.cnc_rdtimer.msec
                };
                
                if (!current_value.Equals(this._lastValue))
                {
                    this.dataChanged(input, current_value);
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