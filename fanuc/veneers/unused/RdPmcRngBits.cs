using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class RdPmcRngBits : Veneer
    {
        public RdPmcRngBits(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                bits = new int[] { -1 }
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success)
            {
                BitArray b = new BitArray(new byte[] { input.response.pmc_rdpmcrng.buf.cdata[0] });
                
                var current_value = new
                {
                    bits = b.Cast<bool>().Select(bit => bit ? 1 : 0).Reverse().ToArray()
                };
                
                await onDataArrivedAsync(input, current_value);
                
                if (current_value.IsDifferentString((object)lastChangedValue))
                {
                    await onDataChangedAsync(input, current_value);
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