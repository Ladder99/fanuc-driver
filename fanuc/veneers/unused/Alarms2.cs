using System.Collections.Generic;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class Alarms2 : Veneer
    {
        public Alarms2(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new 
            {
                alarms = new List<dynamic>() { -1 }
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if(input.success == true)
            {
                var temp_value = new List<dynamic>() ;

                var count = input.response.cnc_rdalmmsg2.num;
                var alms = input.response.cnc_rdalmmsg2.almmsg;
                
                if (count > 0)
                {
                    var fields = alms.GetType().GetFields();
                    for (int x = 0; x <= count - 1; x++)
                    {
                        var alm = fields[x].GetValue(alms);
                        temp_value.Add(
                            new
                            {
                                alm.alm_no, 
                                alm.type, 
                                alm.axis, 
                                alm_msg = ((string)alm.alm_msg).AsAscii()
                            });
                    }
                }
            
                var current_value = new
                {
                    alarms = temp_value
                };
                
                await OnDataArrivedAsync(input, current_value);
                
                if(current_value.alarms.IsDifferentHash((List<dynamic>)lastChangedValue.alarms))
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