using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class RdAxisname: Veneer
    {
        public RdAxisname(string name = "", bool isInternal = false) : base(name, isInternal)
        {
            _lastChangedValue = new
            {
                axes = new List<dynamic>()
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, dynamic? input2)
        {
            if (input.success)
            {
                var temp_value = new List<dynamic>();
                
                var fields = input.response.cnc_rdaxisname.axisname.GetType().GetFields();
                for (int x = 0; x <= input.response.cnc_rdaxisname.data_num - 1; x++)
                {
                    var axis = fields[x].GetValue(input.response.cnc_rdaxisname.axisname);
                    temp_value.Add(new
                    {
                        name = ((char)axis.name).ToString().Trim('\0'), 
                        suff =  ((char)axis.suff).ToString().Trim('\0')
                    });
                }

                var current_value = new
                {
                    axes = temp_value
                };
                
                var current_hc = current_value.axes.Select(x => x.GetHashCode());
                var last_hc = ((List<dynamic>)_lastChangedValue.axes).Select(x => x.GetHashCode());
                
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