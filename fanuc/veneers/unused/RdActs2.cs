using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class RdActs2: Veneer
    {
        public RdActs2(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new 
            {
                data = -1
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success)
            {
                var current_value = new { data = input.response.cnc_acts2.actualspindle.data[0] };
                
                var fields = input.response.cnc_acts2.actualspindle.GetType().GetFields();
                
                await OnDataArrivedAsync(input, current_value);
                
                if(!current_value.Equals(lastChangedValue))
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