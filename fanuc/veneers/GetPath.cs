using l99.driver.@base;

// ReSharper disable once CheckNamespace
namespace l99.driver.fanuc.veneers
{
    public class GetPath : Veneer
    {
        public GetPath(string name = "", bool isCompound = false, bool isInternal = false) : base(name, isCompound, isInternal)
        {
            lastChangedValue = new
            {
                path_no = -1,
                maxpath_no = -1
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, params dynamic?[] additionalInputs)
        {
            if (input.success)
            {
                var currentValue = new
                {
                    input.response.cnc_getpath.path_no,
                    input.response.cnc_getpath.maxpath_no
                };
                
                await OnDataArrivedAsync(input, currentValue);
                
                if (!currentValue.Equals(this.lastChangedValue))
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