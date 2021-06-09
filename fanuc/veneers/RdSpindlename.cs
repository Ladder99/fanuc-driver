using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class RdSpindlename: Veneer
    {
        public RdSpindlename(string name = "", bool isInternal = false) : base(name, isInternal)
        {
            _lastChangedValue = new
            {
                spindles = new List<dynamic>()
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additional_inputs)
        {
            if (input.success)
            {
                var temp_value = new List<dynamic>();
                
                var fields = input.response.cnc_rdspdlname.spdlname.GetType().GetFields();
                for (int x = 0; x <= input.response.cnc_rdspdlname.data_num - 1; x++)
                {
                    var spindle = fields[x].GetValue(input.response.cnc_rdspdlname.spdlname);
                    temp_value.Add(new
                    {
                        name = ((char)spindle.name).AsAscii(), 
                        suff1 =  ((char)spindle.suff1).AsAscii(),
                        suff2 =  ((char)spindle.suff2).AsAscii()
                    });
                }
                
                var current_value = new
                {
                    spindles = temp_value
                };
                
                await onDataArrivedAsync(input, current_value);
                
                if(current_value.spindles.IsDifferentHash((List<dynamic>)_lastChangedValue.spindles))
                    await onDataChangedAsync(input, current_value);
                
                /*
                var current_hc = current_value.spindles.Select(x => x.GetHashCode());
                var last_hc = ((List<dynamic>)_lastChangedValue.spindles).Select(x => x.GetHashCode());
                
                await onDataArrivedAsync(input, current_value);
                
                if(current_hc.Except(last_hc).Count() + last_hc.Except(current_hc).Count() > 0)
                {
                    await onDataChangedAsync(input, current_value);
                }
                */
            }
            else
            {
                await onErrorAsync(input);
            }
            
            return new { veneer = this };
        }
    }
}