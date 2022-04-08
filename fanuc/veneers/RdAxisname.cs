using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class RdAxisname: Veneer
    {
        public RdAxisname(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                axes = new List<dynamic>()
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success)
            {
                var temp_value = new List<dynamic>();
                
                var fields = input.response.cnc_rdaxisname.axisname.GetType().GetFields();
                for (int x = 0; x <= input.response.cnc_rdaxisname.data_num - 1; x++)
                {
                    var axis = fields[x].GetValue(input.response.cnc_rdaxisname.axisname);
                    temp_value.Add(new
                    {
                        name = ((char)axis.name).AsAscii(), 
                        suff =  ((char)axis.suff).AsAscii()
                    });
                }

                var current_value = new
                {
                    axes = temp_value
                };
                
                await onDataArrivedAsync(input, current_value);
                
                if(current_value.axes.IsDifferentHash((List<dynamic>)lastChangedValue.axes))
                    await onDataChangedAsync(input, current_value);
            }
            else
            {
                await onErrorAsync(input);
            }
            
            return new { veneer = this };
        }
    }
}