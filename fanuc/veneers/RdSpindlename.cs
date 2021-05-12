using System.Collections.Generic;
using System.Linq;

namespace fanuc.veneers
{
    public class RdSpindlename: Veneer
    {
        public RdSpindlename(string name = "", bool isInternal = false) : base(name, isInternal)
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
                
                var fields = input.response.cnc_rdspdlname.spdlname.GetType().GetFields();
                for (int x = 0; x <= input.response.cnc_rdspdlname.data_num - 1; x++)
                {
                    var spindle = fields[x].GetValue(input.response.cnc_rdspdlname.spdlname);
                    current_value.Add(new
                    {
                        name = ((char)spindle.name).ToString().Trim('\0'), 
                        suff1 =  ((char)spindle.suff1).ToString().Trim('\0').Trim(),
                        suff2 =  ((char)spindle.suff2).ToString().Trim('\0').Trim(),
                        suff3 =  ((char)spindle.suff3).ToString().Trim('\0').Trim('\u0003').Trim()
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