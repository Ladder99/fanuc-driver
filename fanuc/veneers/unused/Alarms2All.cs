﻿using System.Collections.Generic;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class Alarms2All : Veneer
    {
        public Alarms2All(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            LastChangedValue = new 
            {
                alarms = new List<dynamic>() { -1 }
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
        {
            var success = true;
            var temp_value = new List<dynamic>() ;
            
            foreach (var key in input.response.cnc_rdalmmsg2_ALL.Keys)
            {
                var type_success = input.response.cnc_rdalmmsg2_ALL[key].success;

                if (!type_success)
                {
                    success = false;
                    break;
                }

                var request_data = input.response.cnc_rdalmmsg2_ALL[key].request.cnc_rdalmmsg2;
                var response_data = input.response.cnc_rdalmmsg2_ALL[key].response.cnc_rdalmmsg2;
                var alarm_type = request_data.type;
                var alarm_count = response_data.num;

                if (alarm_count > 0)
                {
                    var fields = response_data.almmsg.GetType().GetFields();
                    for (int x = 0; x <= alarm_count - 1; x++)
                    {
                        var alm = fields[x].GetValue(response_data.almmsg);
                        temp_value.Add(new { alm.alm_no, alm.type, alm.axis, alm_msg = ((string)alm.alm_msg).AsAscii() });
                    }
                }
            }
            
            if (success)
            {
                var current_value = new
                {
                    alarms = temp_value
                };
                
                await OnDataArrivedAsync(input, current_value);
                
                if(current_value.alarms.IsDifferentHash((List<dynamic>)LastChangedValue.alarms))
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