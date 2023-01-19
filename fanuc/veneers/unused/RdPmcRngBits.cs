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
            LastChangedValue = new
            {
                bits = new int[] { -1 }
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
        {
            if (input.success)
            {
                BitArray b = new BitArray(new byte[] { input.response.pmc_rdpmcrng.buf.cdata[0] });
                
                var current_value = new
                {
                    bits = b.Cast<bool>().Select(bit => bit ? 1 : 0).Reverse().ToArray()
                };
                
                await OnDataArrivedAsync(input, current_value);
                
                if (current_value.IsDifferentString((object)LastChangedValue))
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