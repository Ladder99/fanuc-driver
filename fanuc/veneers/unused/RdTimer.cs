using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class RdTimer: Veneer
    {
        public RdTimer(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new 
            {
                minute = -1,
                msec = -1
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success)
            {
                var current_value = new
                {
                    input.response.cnc_rdtimer.time.minute,
                    input.response.cnc_rdtimer.time.msec
                };
                
                await OnDataArrivedAsync(input, current_value);
                
                if (!current_value.Equals(this.lastChangedValue))
                {
                    await OnDataChangedAsync(input, current_value);
                }
            }
            else
            {
                await onErrorAsync(input);
            }

            return new { veneer = this };
        }
    }
}