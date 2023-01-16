using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers
{
    public class RdSpindlename: Veneer
    {
        public RdSpindlename(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                spindles = new List<dynamic>()
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success)
            {
                var tempValue = new List<dynamic>();
                
                var fields = input.response.cnc_rdspdlname.spdlname.GetType().GetFields();
                for (int x = 0; x <= input.response.cnc_rdspdlname.data_num - 1; x++)
                {
                    var spindle = fields[x].GetValue(input.response.cnc_rdspdlname.spdlname);
                    tempValue.Add(new
                    {
                        name = ((char)spindle.name).AsAscii(), 
                        suff1 =  ((char)spindle.suff1).AsAscii(),
                        suff2 =  ((char)spindle.suff2).AsAscii()
                    });
                }
                
                var currentValue = new
                {
                    spindles = tempValue
                };
                
                await OnDataArrivedAsync(input, currentValue);

                if (currentValue.spindles.IsDifferentHash((List<dynamic>) lastChangedValue.spindles))
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