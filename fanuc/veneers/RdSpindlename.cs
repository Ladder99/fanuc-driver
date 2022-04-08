using l99.driver.@base;

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
                var temp_value = new List<dynamic>();
                
                var fields = input.response.cnc_rdspdlname.spdlname.GetType().GetFields();
                for (int x = 0; x <= input.response.cnc_rdspdlname.data_num - 1; x++)
                {
                    var spindle = fields[x].GetValue(input.response.cnc_rdspdlname.spdlname);
                    temp_value.Add(new
                    {
                        name = ((char)spindle.name).AsAscii(), 
                        suff1 =  ((char)spindle.suff1).AsAscii(),
                        suff2 =  ((char)spindle.suff2).AsAscii()
                    });
                }
                
                var current_value = new
                {
                    spindles = temp_value
                };
                
                await onDataArrivedAsync(input, current_value);

                if (current_value.spindles.IsDifferentHash((List<dynamic>) lastChangedValue.spindles))
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