using l99.driver.@base;

// ReSharper disable once CheckNamespace
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
                var tempValue = new List<dynamic>();

                var fields = input.response.cnc_rdopmsg.opmsg.GetType().GetFields();
                for (int x = 0; x <= fields.Length - 1; x++)
                {
                    var msg = fields[x].GetValue(input.response.cnc_rdopmsg.opmsg);
                    if (msg.char_num > 0)
                    {
                        tempValue.Add(new
                        {
                            position = msg.type,
                            number = msg.datano,
                            message = msg.data
                        });
                    }
                }

                var currentValue = new
                {
                    messages = tempValue
                };
                
                var currentHc = currentValue.messages.Select(x => x.GetHashCode());
                var lastHc = ((List<dynamic>)lastChangedValue.messages).Select(x => x.GetHashCode());
                
                await OnDataArrivedAsync(input, currentValue);
                
                if(currentHc.Except(lastHc).Count() + lastHc.Except(currentHc).Count() > 0)
                {
                    await OnDataChangedAsync(input, currentValue);
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