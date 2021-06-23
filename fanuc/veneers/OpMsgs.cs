using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class OpMsgs: Veneer
    {
        public OpMsgs(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                msgs = new List<dynamic>()
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success)
            {
                var temp_value = new List<dynamic>();

                var fields = input.response.cnc_rdopmsg.opmsg.GetType().GetFields();
                for (int x = 0; x <= fields.Length - 1; x++)
                {
                    var msg = fields[x].GetValue(input.response.cnc_rdopmsg.opmsg);
                    if (msg.char_num > 0)
                    {
                        temp_value.Add(new
                        {
                            msg.data,
                            msg.datano,
                            msg.type
                        });
                    }
                }

                var current_value = new
                {
                    msgs = temp_value
                };
                
                var current_hc = current_value.msgs.Select(x => x.GetHashCode());
                var last_hc = ((List<dynamic>)lastChangedValue.msgs).Select(x => x.GetHashCode());
                
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