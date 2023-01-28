using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers
{
    public class GetPath : Veneer
    {
        public GetPath(Veneers veneers, string name = "", bool isCompound = false, bool isInternal = false) : base(veneers, name, isCompound, isInternal)
        {
            
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic[] nativeInputs, dynamic[] additionalInputs)
        {
            if (nativeInputs.All(o => o.success == true))
            {
                var currentValue = new
                {
                    nativeInputs[0].response.cnc_getpath.path_no,
                    nativeInputs[0].response.cnc_getpath.maxpath_no
                };
                
                await OnDataArrivedAsync(nativeInputs, additionalInputs, currentValue);
                
                //if (!currentValue.Equals(LastChangedValue))
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