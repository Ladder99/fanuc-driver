using System.Threading.Tasks;
using l99.driver.@base;

namespace l99.driver.fanuc.veneers
{
    public class Connect : Veneer
    {
        public Connect(string name = "", bool isInternal = false) : base(name, isInternal)
        {

        }

        protected override async Task<dynamic> FirstAsync(dynamic input, dynamic? input2)
        {
            var current_value = new {input.success};
            
            await onDataArrivedAsync(input, current_value);
            await onDataChangedAsync(input, current_value);

            return new { veneer = this };
        }

        protected override async Task<dynamic> AnyAsync(dynamic input, dynamic? input2)
        {
            var current_value = new {input.success };
            
            await onDataArrivedAsync(input, current_value);
            
            if (!current_value.Equals(_lastChangedValue))
            {
                await onDataChangedAsync(input, current_value);
            }
            
            return new { veneer = this };
        }
    }
}