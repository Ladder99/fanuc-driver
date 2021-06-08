using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class SpindleData: Veneer
    {
        public SpindleData(string name = "", bool isInternal = false) : base(name, isInternal)
        {
            _lastChangedValue = new
            {
                name = string.Empty,
                
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additional_inputs)
        {
            if (input.success)
            {
                var spindle_fields = additional_inputs[0].response.cnc_rdspdlname.spdlname.GetType().GetFields();
                var spindle_value = spindle_fields[input - 1].GetValue(input.response.cnc_rdspdlname.spdlname);
                var spindle_name = ((char) spindle_value.name).AsAscii() +
                                   ((char) spindle_value.suff1).AsAscii() +
                                   ((char) spindle_value.suff2).AsAscii() +
                                   ((char) spindle_value.suff3).AsAscii();


                var current_value = new
                {
                    name = spindle_name
                };
                
                await onDataArrivedAsync(input, current_value);
                
                if(current_value.IsDifferentString((object)_lastChangedValue))
                    await onDataChangedAsync(input, current_value);
            }
            else
            {
                await onErrorAsync(input);
            }
            
            return new { veneer = this };
        }
    }
}