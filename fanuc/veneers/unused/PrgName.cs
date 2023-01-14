using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class PrgName : Veneer
    {
        public PrgName(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                name = string.Empty,
                number = -1
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success)
            {
                var current_value = new
                {
                    name = new string(input.response.cnc_exeprgname.exeprg.name).AsAscii(),
                    number = input.response.cnc_exeprgname.exeprg.o_num
                };
                
                await OnDataArrivedAsync(input, current_value);
                
                if (!current_value.Equals(lastChangedValue))
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