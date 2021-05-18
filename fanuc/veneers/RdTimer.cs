using System.Collections.Generic;
using System.Linq;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class RdTimer: Veneer
    {
        public RdTimer(string name = "", bool isInternal = false) : base(name, isInternal)
        {
            _lastChangedValue = new 
            {
                minute = -1,
                msec = -1
            };
        }
        
        protected override dynamic Any(dynamic input, dynamic? input2)
        {
            if (input.success)
            {
                var current_value = new
                {
                    input.response.cnc_rdtimer.time.minute,
                    input.response.cnc_rdtimer.time.msec
                };
                
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