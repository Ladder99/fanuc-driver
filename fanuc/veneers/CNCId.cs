using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class CNCId : Veneer
    {
        public CNCId(string name = "", bool isInternal = false) : base(name, isInternal)
        {
            _lastChangedValue = new
            {
                cncid = string.Empty
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additional_inputs)
        {
            if (input.success)
            {;
                var current_value = new
                {
                    cncid = string.Join("-", ((uint[])input.response.cnc_rdcncid.cncid).Select(x => x.ToString("X")).ToArray())
                };
                
                await onDataArrivedAsync(input, current_value);
                
                if (!current_value.Equals(_lastChangedValue))
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