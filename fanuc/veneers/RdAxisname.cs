using System.Collections.Generic;
using System.Linq;

namespace fanuc.veneers
{
    public class RdAxisname: Veneer
    {
        public RdAxisname(string name = "", bool isInternal = false) : base(name, isInternal)
        {
            _lastChangedValue = new List<dynamic>
            {
                
            };
        }
        
        protected override dynamic Any(dynamic input)
        {
            if (input.success)
            {
                var current_value = new List<dynamic>();
                
                var fields = input.response.cnc_rdaxisname.axisname.GetType().GetFields();
                for (int x = 0; x <= input.response.cnc_rdaxisname.data_num - 1; x++)
                {
                    var axis = fields[x].GetValue(input.response.cnc_rdaxisname.axisname);
                    current_value.Add(new
                    {
                        name = ((char)axis.name).ToString().Trim('\0'), 
                        suff =  ((char)axis.suff).ToString().Trim('\0')
                    });
                }
                
                var current_hc = current_value.Select(x => x.GetHashCode());
                var last_hc = ((List<dynamic>)_lastChangedValue).Select(x => x.GetHashCode());
                
                this.onDataArrived(input, current_value);
                
                if(current_hc.Except(last_hc).Count() + last_hc.Except(current_hc).Count() > 0)
                {
                    this.onDataChanged(input, current_value);
                }
            }
            else
            {
                onError(input);
            }
            
            return new { veneer = this };
        }
    }
}