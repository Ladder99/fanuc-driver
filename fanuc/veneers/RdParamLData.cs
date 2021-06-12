using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class RdParamLData : Veneer
    {
        public RdParamLData(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            _lastChangedValue = new
            {
                ldata = -1
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additional_inputs)
        {
            if (input.success)
            {
                //var current_value = new { ldata = input.response.cnc_rdparam.param.ldata };
                
                var current_value = new { ldata = input.response.cnc_rdparam.param.data.ldata };
                
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