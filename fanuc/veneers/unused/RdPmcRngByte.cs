using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class RdPmcRngByte : Veneer
    {
        public RdPmcRngByte(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                cdata = -1
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success)
            {
                var current_value = new
                {
                    cdata = input.response.pmc_rdpmcrng.buf.cdata[0]
                };
                
                await OnDataArrivedAsync(input, current_value);
                
                if (!current_value.Equals(lastChangedValue))
                {
                    await OnDataChangedAsync(input, current_value);
                }
            }
            else
            {
                await OnHandleErrorAsync(input);
            }

            return new { veneer = this };
        }
    }
}