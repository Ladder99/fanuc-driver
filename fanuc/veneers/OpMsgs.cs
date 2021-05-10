using System.Collections.Generic;
using System.Linq;

namespace fanuc.veneers
{
    public class OpMsgs: Veneer
    {
        public OpMsgs(string name = ""): base(name)
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

                var fields = input.response.cnc_rdopmsg.opmsg.GetType().GetFields();
                for (int x = 0; x <= fields.Length - 1; x++)
                {
                    var msg = fields[x].GetValue(input.response.cnc_rdopmsg.opmsg);
                    if (msg.char_num > 0)
                    {
                        current_value.Add(new
                        {
                            msg.data,
                            msg.datano,
                            msg.type
                        });
                    }
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