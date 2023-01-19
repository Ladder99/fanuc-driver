using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class RdPmcRngWord : Veneer
    {
        public RdPmcRngWord(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            LastChangedValue = new
            {
                idata = -1
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
        {
            if (input.success)
            {
                var current_value = new
                {
                    idata = input.response.pmc_rdpmcrng.buf.idata[0]
                };
                
                await OnDataArrivedAsync(input, current_value);
                
                if (!current_value.Equals(LastChangedValue))
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