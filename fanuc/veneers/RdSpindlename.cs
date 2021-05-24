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
        
        protected override async Task<dynamic> AnyAsync(dynamic input, dynamic? input2)
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
                        name = ((char)spindle.name).ToString().Trim('\0'), 
                        suff1 =  ((char)spindle.suff1).ToString().Trim('\0').Trim(),
                        suff2 =  ((char)spindle.suff2).ToString().Trim('\0').Trim(),
                        suff3 =  ((char)spindle.suff3).ToString().Trim('\0').Trim('\u0003').Trim('\u0001').Trim()
                    });
                }
                
                var current_value = new
                {
                    spindles = temp_value
                };
                
                var current_hc = current_value.spindles.Select(x => x.GetHashCode());
                var last_hc = ((List<dynamic>)_lastChangedValue.spindles).Select(x => x.GetHashCode());
                
                await onDataArrivedAsync(input, current_value);
                
                if(current_hc.Except(last_hc).Count() + last_hc.Except(current_hc).Count() > 0)
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