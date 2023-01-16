using l99.driver.@base;

// ReSharper disable once CheckNamespace
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
                var tempValue = new List<dynamic>();
                
                var fields = input.response.cnc_rdaxisname.axisname.GetType().GetFields();
                for (int x = 0; x <= input.response.cnc_rdaxisname.data_num - 1; x++)
                {
                    var axis = fields[x].GetValue(input.response.cnc_rdaxisname.axisname);
                    tempValue.Add(new
                    {
                        name = ((char)axis.name).AsAscii(), 
                        suff =  ((char)axis.suff).AsAscii()
                    });
                }

                var currentValue = new
                {
                    axes = tempValue
                };
                
                await OnDataArrivedAsync(input, currentValue);
                
                if(currentValue.axes.IsDifferentHash((List<dynamic>)lastChangedValue.axes))
                    await OnDataChangedAsync(input, currentValue);
            }
            else
            {
                await OnHandleErrorAsync(input);
            }
            
            return new { veneer = this };
        }
    }
}