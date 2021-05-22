using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class GetPath : Veneer
    {
        public GetPath(string name = "", bool isInternal = false) : base(name, isInternal)
        {
            _lastChangedValue = new
            {
                path_no = -1,
                maxpath_no = -1
            };
        }
        
        protected override async Task<dynamic> AnyAsync(dynamic input, dynamic? input2)
        {
            if (input.success)
            {
                var current_value = new
                {
                    input.response.cnc_getpath.path_no,
                    input.response.cnc_getpath.maxpath_no
                };
                
                await onDataArrivedAsync(input, current_value);
                
                if (!current_value.Equals(this._lastChangedValue))
                {
                    await onDataChangedAsync(input, current_value);
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