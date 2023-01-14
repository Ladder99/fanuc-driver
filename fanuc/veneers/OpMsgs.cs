using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class OpMsgs: Veneer
    {
        public OpMsgs(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                messages = new List<dynamic>() { -1 }
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
                            position = msg.type,
                            number = msg.datano,
                            message = msg.data
                        });
                    }
                }

                var current_value = new
                {
                    messages = temp_value
                };
                
                var current_hc = current_value.messages.Select(x => x.GetHashCode());
                var last_hc = ((List<dynamic>)lastChangedValue.messages).Select(x => x.GetHashCode());
                
                await OnDataArrivedAsync(input, current_value);
                
                if(current_hc.Except(last_hc).Count() + last_hc.Except(current_hc).Count() > 0)
                {
                    await OnDataChangedAsync(input, current_value);
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