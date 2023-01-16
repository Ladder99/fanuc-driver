using System.Collections.Generic;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class ThreeWayAlarmData : Veneer
    {
        public ThreeWayAlarmData(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new 
            {
                alarms = new List<dynamic>() { -1 }
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            var success = true;
            var temp_value = new List<dynamic>() ;
            
            foreach (var key in input.response.cnc_rdalmmsg_ALL.Keys)
            {
                var type_success = input.response.cnc_rdalmmsg_ALL[key].success;

                if (!type_success)
                {
                    success = false;
                    break;
                }

                var response_data = input.response.cnc_rdalmmsg_ALL[key].response.cnc_rdalmmsg;
                var alarm_count = response_data.num;

                if (alarm_count > 0)
                {
                    var alm_fields = response_data.almmsg.GetType().GetFields();
                    for (int x = 0; x <= alarm_count - 1; x++)
                    {
                        var alm = alm_fields[x].GetValue(response_data.almmsg);
                        temp_value.Add(new
                        {
                            kind = "alarm", 
                            number = alm.alm_no, 
                            type = alm.type, 
                            axis = alm.axis, 
                            message = ((string)alm.alm_msg).AsAscii()
                        });
                    }
                }
            }
            
            //TODO: check success
            var msg_fields = additionalInputs[0].response.cnc_rdopmsg.opmsg.GetType().GetFields();
            for (int x = 0; x <= msg_fields.Length - 1; x++)
            {
                var msg = msg_fields[x].GetValue(additionalInputs[0].response.cnc_rdopmsg.opmsg);
                if (msg.char_num > 0)
                {
                    temp_value.Add(new
                    {
                        kind = "message",
                        number = msg.datano,
                        type = msg.type,
                        axis = -1,
                        message = msg.data
                    });
                }
            }
            
            if (success)
            {
                var current_value = new
                {
                    alarms = temp_value
                };
                
                await OnDataArrivedAsync(input, current_value);

                if (current_value.alarms.IsDifferentHash((List<dynamic>) lastChangedValue.alarms))
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