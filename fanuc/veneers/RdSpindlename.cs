using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers
{
    public class RdSpindlename: Veneer
    {
        public RdSpindlename(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false) : base(veneers, name, isCompound, isInternal)
        {
            
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
        {
            if (nativeInputs.All(o => o.success == true))
            {
                var tempValue = new List<dynamic>();
                
                var fields = nativeInputs[0].response.cnc_rdspdlname.spdlname.GetType().GetFields();
                for (int x = 0; x <= nativeInputs[0].response.cnc_rdspdlname.data_num - 1; x++)
                {
                    var spindle = fields[x].GetValue(nativeInputs[0].response.cnc_rdspdlname.spdlname);
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
                
                await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);

                if (currentValue.IsDifferentString((object) LastChangedValue))
                {
                    await OnDataChangedAsync(nativeInputs, additionalInputs, currentValue);
                }
            }
            else
            {
                await OnHandleErrorAsync(nativeInputs, additionalInputs);
            }
            
            return new { veneer = this };
        }
    }
}