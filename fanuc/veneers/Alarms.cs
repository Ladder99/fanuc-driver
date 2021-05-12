using System.Collections.Generic;
using System.Linq;

namespace fanuc.veneers
{
    public class Alarms : Veneer
    {
        public Alarms(string name = "", bool isInternal = false) : base(name, isInternal)
        {
            _lastChangedValue = new List<dynamic>
            {
                
            };
        }
        
        protected override dynamic Any(dynamic input, dynamic? input2)
        {
            var success = true;
            var current_value = new List<dynamic>() ;
            
            foreach (var key in input.response.cnc_rdalmmsg_ALL.Keys)
            {
                var type_success = input.response.cnc_rdalmmsg_ALL[key].success;

                if (!type_success)
                {
                    success = false;
                    break;
                }

                var request_data = input.response.cnc_rdalmmsg_ALL[key].request.cnc_rdalmmsg;
                var response_data = input.response.cnc_rdalmmsg_ALL[key].response.cnc_rdalmmsg;
                var alarm_type = request_data.type;
                var alarm_count = response_data.num;

                if (alarm_count > 0)
                {
                    var fields = response_data.almmsg.GetType().GetFields();
                    for (int x = 0; x <= alarm_count - 1; x++)
                    {
                        var alm = fields[x].GetValue(response_data.almmsg);
                        current_value.Add(new { alm.alm_no, alm.type, alm.axis, alm_msg = alm.alm_msg.Trim('\u0001') });
                    }
                }
            }
            
            if (success)
            {
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